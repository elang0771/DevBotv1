package com.elang.reminderwakesend;

import android.accessibilityservice.AccessibilityService;
import android.accessibilityservice.GestureDescription;
import android.content.SharedPreferences;
import android.graphics.Path;
import android.graphics.Rect;
import android.os.Handler;
import android.os.Looper;
import android.view.accessibility.AccessibilityEvent;
import android.view.accessibility.AccessibilityNodeInfo;
import java.util.Locale;

/**
 * Only active during the user's scheduled, single-recipient test.
 * Never click blindly: require scheduled caption, recognizable WhatsApp
 * media composer and visible Send control at lower-right.
 */
public class AutoSendAccessibilityService extends AccessibilityService {
    private static final long RETRY_MS = 1200L;
    private final Handler handler = new Handler(Looper.getMainLooper());
    private boolean checking;
    private long lastJob;
    private long lastWaitLog;
    private boolean seenLogged;
    private boolean captionLogged;
    private boolean previewLogged;

    @Override
    public void onAccessibilityEvent(AccessibilityEvent event) {
        if (event == null || event.getPackageName() == null
                || !MainActivity.WA.contentEquals(event.getPackageName())) return;
        SharedPreferences prefs = Diag.prefs(this);
        long armedUntil = prefs.getLong("armed_until", 0L);
        if (armedUntil == 0L) return;
        if (armedUntil <= System.currentTimeMillis()) {
            expire();
            return;
        }
        if (lastJob != armedUntil) {
            lastJob = armedUntil;
            seenLogged = false;
            captionLogged = false;
            previewLogged = false;
            lastWaitLog = 0L;
        }
        if (!seenLogged) {
            Diag.log(this, "WHATSAPP_ACCESSIBILITY_OBSERVED");
            seenLogged = true;
        }
        if (!checking) {
            checking = true;
            handler.postDelayed(this::inspectAndSend, 900L);
        }
    }

    private void inspectAndSend() {
        SharedPreferences prefs = Diag.prefs(this);
        long deadline = prefs.getLong("armed_until", 0L);
        if (deadline == 0L) { checking = false; return; }
        if (deadline <= System.currentTimeMillis()) { expire(); return; }
        if (prefs.getBoolean("click_in_progress", false)) { checking = false; return; }

        AccessibilityNodeInfo root = getRootInActiveWindow();
        if (root == null || root.getPackageName() == null
                || !MainActivity.WA.contentEquals(root.getPackageName())) {
            waitAndRetry("WAITING_WHATSAPP_FOREGROUND");
            return;
        }

        String caption = prefs.getString("expected_caption", "");
        if (caption.isEmpty()) {
            Diag.disarm(this, "CANCELLED_EMPTY_EXPECTED_CAPTION");
            checking = false;
            return;
        }
        // Use scheduled caption, not an arbitrary WhatsApp Send button.
        String signature = caption.length() > 24 ? caption.substring(0, 24) : caption;
        Snapshot snapshot = new Snapshot();
        scan(root, signature, snapshot, 0);

        if (!snapshot.captionMatched) {
            waitAndRetry("WAITING_SCHEDULED_CAPTION");
            return;
        }
        if (!captionLogged) {
            Diag.log(this, "CAPTION_VISIBLE_MATCHED");
            captionLogged = true;
        }

        // A chat containing the caption is not necessarily the media preview!
        boolean preview = snapshot.strongMediaMarker
                || (snapshot.captionEditable && snapshot.mediaToolbarMarker && snapshot.largeImage);
        if (!preview) {
            waitAndRetry("CAPTION_FOUND_WAITING_MEDIA_PREVIEW markers="
                    + snapshot.markerCount + " editable=" + snapshot.captionEditable);
            return;
        }
        if (!previewLogged) {
            Diag.log(this, "MEDIA_PREVIEW_CONFIRMED markers=" + snapshot.markerCount);
            previewLogged = true;
        }
        AccessibilityNodeInfo send = snapshot.sendCandidate;
        if (send == null) {
            waitAndRetry("MEDIA_PREVIEW_READY_SEND_NOT_FOUND candidates=" + snapshot.sendCount);
            return;
        }
        Rect bounds = new Rect();
        send.getBoundsInScreen(bounds);
        if (!lowerRight(bounds)) {
            waitAndRetry("SEND_CONTROL_NOT_IN_EXPECTED_POSITION");
            return;
        }

        prefs.edit().putBoolean("click_in_progress", true).commit();
        boolean clicked = clickNodeOrParent(send);
        if (clicked) {
            // UI click accepted != WhatsApp message delivered.
            Diag.disarm(this, "SEND_CLICK_ACCEPTED_NOT_DELIVERY_PROOF");
            checking = false;
            return;
        }

        // Fallback uses accessibility-reported bounds, NEVER fixed coordinates.
        if (bounds.isEmpty() || !send.isVisibleToUser()) {
            prefs.edit().putBoolean("click_in_progress", false).commit();
            waitAndRetry("SEND_CONTROL_NOT_ACTIONABLE");
            return;
        }
        Path path = new Path();
        path.moveTo(bounds.centerX(), bounds.centerY());
        GestureDescription gesture = new GestureDescription.Builder()
                .addStroke(new GestureDescription.StrokeDescription(path, 0, 75)).build();
        boolean dispatched = dispatchGesture(gesture, new GestureResultCallback() {
            @Override public void onCompleted(GestureDescription g) {
                Diag.disarm(AutoSendAccessibilityService.this,
                        "SEND_GESTURE_COMPLETED_NOT_DELIVERY_PROOF");
            }
            @Override public void onCancelled(GestureDescription g) {
                Diag.disarm(AutoSendAccessibilityService.this, "SEND_GESTURE_CANCELLED");
            }
        }, handler);
        if (!dispatched) {
            Diag.disarm(this, "SEND_GESTURE_DISPATCH_FAILED");
        } else {
            Diag.log(this, "SEND_GESTURE_REQUEST_ACCEPTED");
        }
        // Do not repeat a possibly successful gesture: duplicate-send prevention.
        checking = false;
    }

    private static final class Snapshot {
        boolean captionMatched;
        boolean captionEditable;
        boolean strongMediaMarker;
        boolean mediaToolbarMarker;
        boolean largeImage;
        int markerCount;
        int sendCount;
        AccessibilityNodeInfo sendCandidate;
        int sendScore = Integer.MIN_VALUE;
    }

    private void scan(AccessibilityNodeInfo node, String signature, Snapshot s, int depth) {
        if (node == null || depth > 35 || !node.isVisibleToUser()) return;
        String id = safe(node.getViewIdResourceName()).toLowerCase(Locale.ROOT);
        String desc = safe(node.getContentDescription()).toLowerCase(Locale.ROOT);
        String txt = safe(node.getText());
        String cls = safe(node.getClassName()).toLowerCase(Locale.ROOT);
        String lowerText = txt.toLowerCase(Locale.ROOT);

        if (txt.contains(signature)) {
            s.captionMatched = true;
            if (node.isEditable() || cls.endsWith("edittext")
                    || id.contains("caption")) s.captionEditable = true;
        }
        if (id.contains("media_preview") || id.contains("photo_preview")
                || id.contains("image_preview") || id.contains("media_composer")
                || id.contains("media_caption") || id.contains("media_edit")) {
            s.strongMediaMarker = true;
            s.markerCount++;
        }
        if (lowerText.contains("restyle with ai") || desc.contains("restyle with ai")
                || id.contains("crop") || id.contains("media_editor")
                || id.contains("photo_editor") || id.contains("caption_input")
                || id.contains("media_viewer")) {
            s.mediaToolbarMarker = true;
            s.markerCount++;
        }
        Rect r = new Rect();
        node.getBoundsInScreen(r);
        int screenWidth = getResources().getDisplayMetrics().widthPixels;
        int screenHeight = getResources().getDisplayMetrics().heightPixels;
        if (cls.contains("imageview") && r.width() > screenWidth * 0.48
                && r.height() > screenHeight * 0.22) s.largeImage = true;

        boolean sendId = id.endsWith("/send") || id.endsWith("/send_button")
                || id.contains("send_media") || id.contains("media_send")
                || id.endsWith("/send_media_button") || id.endsWith("/send_media");
        boolean sendLabel = desc.equals("send") || desc.equals("kirim")
                || desc.equals("send photo") || desc.equals("send media")
                || desc.equals("send image") || desc.equals("kirim foto")
                || desc.equals("kirim media")
                || lowerText.equals("send") || lowerText.equals("kirim");
        if ((sendId || sendLabel) && lowerRight(r)) {
            s.sendCount++;
            int score = (sendId ? 3 : 0) + (sendLabel ? 2 : 0)
                    + (node.isClickable() ? 2 : 0);
            if (score > s.sendScore) {
                s.sendScore = score;
                s.sendCandidate = node;
            }
        }
        for (int i = 0; i < node.getChildCount(); i++) {
            scan(node.getChild(i), signature, s, depth + 1);
        }
    }

    private boolean lowerRight(Rect r) {
        if (r == null || r.isEmpty()) return false;
        int w = getResources().getDisplayMetrics().widthPixels;
        int h = getResources().getDisplayMetrics().heightPixels;
        return r.centerX() > (int)(w * 0.57f)
                && r.centerY() > (int)(h * 0.58f);
    }

    private boolean clickNodeOrParent(AccessibilityNodeInfo node) {
        AccessibilityNodeInfo current = node;
        for (int i = 0; i < 5 && current != null; i++) {
            if (current.isClickable()
                    && current.performAction(AccessibilityNodeInfo.ACTION_CLICK)) return true;
            current = current.getParent();
        }
        return false;
    }

    private void waitAndRetry(String message) {
        long now = System.currentTimeMillis();
        if (now - lastWaitLog >= 6000) {
            Diag.log(this, message);
            lastWaitLog = now;
        }
        handler.postDelayed(this::inspectAndSend, RETRY_MS);
    }

    private void expire() {
        checking = false;
        Diag.disarm(this, "TIMEOUT_NO_AUTO_SEND");
    }

    private String safe(CharSequence s) { return s == null ? "" : s.toString().trim(); }

    @Override public void onInterrupt() { checking = false; }

    @Override public void onDestroy() {
        handler.removeCallbacksAndMessages(null);
        super.onDestroy();
    }
}
