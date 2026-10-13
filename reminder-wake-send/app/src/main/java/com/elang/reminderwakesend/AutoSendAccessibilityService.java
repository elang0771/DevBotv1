package com.elang.reminderwakesend;

import android.accessibilityservice.AccessibilityService;
import android.accessibilityservice.GestureDescription;
import android.content.SharedPreferences;
import android.graphics.Path;
import android.graphics.Bitmap;
import android.graphics.Color;
import android.graphics.Point;
import android.hardware.HardwareBuffer;
import android.os.Build;
import android.view.Display;
import android.view.WindowManager;
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
            // WhatsApp's newer image editor may expose the caption but hide its
            // toolbar/media markers from Accessibility. Verify it visually.
            inspectScreenshotForMediaSend(signature, deadline);
            return;
        }
        if (!previewLogged) {
            Diag.log(this, "MEDIA_PREVIEW_CONFIRMED markers=" + snapshot.markerCount);
            previewLogged = true;
        }
        AccessibilityNodeInfo send = snapshot.sendCandidate;
        if (send == null) {
            // No clickable Accessibility node: rely on verified screen pixels,
            // never on a hardcoded coordinate or chat-level Send button.
            inspectScreenshotForMediaSend(signature, deadline);
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


    /**
     * Local-only visual verification for WhatsApp's media preview. No captured
     * screenshot is saved, transmitted, logged or exposed to the UI.
     * Without a matching caption AND a dark media editor AND a green round
     * bottom-right send control, no tap is performed.
     */
    private void inspectScreenshotForMediaSend(String signature, long jobDeadline) {
        if (Build.VERSION.SDK_INT < 30) {
            waitAndRetry("SCREENSHOT_API_UNAVAILABLE");
            return;
        }
        try {
            takeScreenshot(Display.DEFAULT_DISPLAY, getMainExecutor(), new TakeScreenshotCallback() {
                @Override public void onSuccess(ScreenshotResult shot) {
                    HardwareBuffer buffer = shot.getHardwareBuffer();
                    Bitmap bitmap = null;
                    try {
                        Bitmap wrapped = Bitmap.wrapHardwareBuffer(buffer, shot.getColorSpace());
                        if (wrapped != null) {
                            bitmap = wrapped.copy(Bitmap.Config.ARGB_8888, false);
                        }
                        if (bitmap == null) {
                            waitAndRetry("SCREENSHOT_BITMAP_UNAVAILABLE");
                            return;
                        }
                        if (Diag.prefs(AutoSendAccessibilityService.this)
                                .getLong("armed_until", 0L) != jobDeadline) {
                            checking = false;
                            return;
                        }
                        if (jobDeadline <= System.currentTimeMillis()) {
                            expire();
                            return;
                        }
                        AccessibilityNodeInfo current = getRootInActiveWindow();
                        if (current == null || current.getPackageName() == null
                                || !MainActivity.WA.contentEquals(current.getPackageName())) {
                            waitAndRetry("SCREENSHOT_WAITING_WHATSAPP");
                            return;
                        }
                        Snapshot refreshed = new Snapshot();
                        scan(current, signature, refreshed, 0);
                        if (!refreshed.captionMatched) {
                            waitAndRetry("SCREENSHOT_CAPTION_NOT_VISIBLE");
                            return;
                        }
                        GreenTarget target = detectMediaSend(bitmap);
                        if (target == null) {
                            waitAndRetry("SCREENSHOT_MEDIA_SEND_NOT_VERIFIED");
                            return;
                        }
                        if (!previewLogged) {
                            Diag.log(AutoSendAccessibilityService.this,
                                    "MEDIA_PREVIEW_VISUALLY_VERIFIED");
                            previewLogged = true;
                        }
                        WindowManager wm = (WindowManager) getSystemService(WINDOW_SERVICE);
                        Point real = new Point();
                        wm.getDefaultDisplay().getRealSize(real);
                        int px = Math.round(target.x * ((float)real.x / bitmap.getWidth()));
                        int py = Math.round(target.y * ((float)real.y / bitmap.getHeight()));
                        Diag.log(AutoSendAccessibilityService.this,
                                "SEND_BUTTON_VISUALLY_LOCATED");
                        performVerifiedGesture(px, py);
                    } catch (Exception err) {
                        waitAndRetry("SCREENSHOT_ANALYSIS_ERROR:" +
                                err.getClass().getSimpleName());
                    } finally {
                        if (bitmap != null) bitmap.recycle();
                        if (buffer != null) buffer.close();
                    }
                }

                @Override public void onFailure(int errorCode) {
                    waitAndRetry("SCREENSHOT_PERMISSION_OR_CAPTURE_FAILED:" + errorCode);
                }
            });
        } catch (Exception e) {
            waitAndRetry("SCREENSHOT_START_FAILED:" + e.getClass().getSimpleName());
        }
    }

    private static final class GreenTarget {
        final int x, y;
        GreenTarget(int x, int y) { this.x = x; this.y = y; }
    }

    private GreenTarget detectMediaSend(Bitmap bmp) {
        final int w=bmp.getWidth(), h=bmp.getHeight();
        if (w < 300 || h < 550) return null;
        // Media editor in the screenshot is black above the photo. A normal
        // WhatsApp chat screen has a header instead, even in light theme.
        int dark=0;
        int ty=Math.round(h*0.175f);
        for (float xx : new float[]{0.16f,0.34f,0.64f,0.85f}) {
            int c=bmp.getPixel(Math.round(w*xx),ty);
            if (Color.red(c)<31 && Color.green(c)<31 && Color.blue(c)<31) dark++;
        }
        if (dark < 3) return null;

        // Locate the actual green circular send button, not an inferred
        // fixed position. Use relative ROI to support phone/tablet sizes.
        int minX=w,minY=h,maxX=-1,maxY=-1,count=0;
        int step=Math.max(2,w/260);
        for (int y=(int)(h*0.77f);y<(int)(h*0.965f);y+=step) {
            for (int x=(int)(w*0.79f);x<(int)(w*0.99f);x+=step) {
                int c=bmp.getPixel(x,y);
                int red=Color.red(c),green=Color.green(c),blue=Color.blue(c);
                if (green>115 && green>red*1.45f && green>blue*1.20f
                        && red<155 && blue<185) {
                    count++;
                    if(x<minX)minX=x;
                    if(x>maxX)maxX=x;
                    if(y<minY)minY=y;
                    if(y>maxY)maxY=y;
                }
            }
        }
        if (count<24) return null;
        int bw=maxX-minX,bh=maxY-minY;
        if (bw<w*0.045f || bw>w*0.19f
                || bh<h*0.022f || bh>h*0.12f) return null;
        float ratio=(float)bw/Math.max(1,bh);
        if (ratio<0.65f||ratio>1.50f) return null;
        int cx=(minX+maxX)/2,cy=(minY+maxY)/2;
        if (cx<w*0.82f || cy<h*0.81f || cy>h*0.965f) return null;
        return new GreenTarget(cx,cy);
    }

    private void performVerifiedGesture(int x,int y) {
        SharedPreferences prefs=Diag.prefs(this);
        if (prefs.getBoolean("click_in_progress",false) ||
                prefs.getLong("armed_until",0L)<=System.currentTimeMillis()) {
            checking=false;
            return;
        }
        // Disallow accidental duplicate sends before the asynchronous gesture.
        prefs.edit().putBoolean("click_in_progress",true).commit();
        Path p=new Path();
        p.moveTo(x,y);
        GestureDescription gesture=new GestureDescription.Builder()
                .addStroke(new GestureDescription.StrokeDescription(p,0,85)).build();
        try {
            boolean requested=dispatchGesture(gesture,new GestureResultCallback() {
                @Override public void onCompleted(GestureDescription g) {
                    Diag.disarm(AutoSendAccessibilityService.this,
                            "SEND_GESTURE_COMPLETED_NOT_DELIVERY_PROOF");
                }
                @Override public void onCancelled(GestureDescription g) {
                    Diag.disarm(AutoSendAccessibilityService.this,
                            "SEND_GESTURE_CANCELLED");
                }
            },handler);
            if (!requested) Diag.disarm(this,"SEND_GESTURE_DISPATCH_FAILED");
            else Diag.log(this,"SEND_GESTURE_REQUEST_ACCEPTED");
        } catch(Exception err) {
            Diag.disarm(this,"SEND_GESTURE_EXCEPTION:"+
                    err.getClass().getSimpleName());
        }
        checking=false;
    }

    private String safe(CharSequence s) { return s == null ? "" : s.toString().trim(); }

    @Override public void onInterrupt() { checking = false; }

    @Override public void onDestroy() {
        handler.removeCallbacksAndMessages(null);
        super.onDestroy();
    }
}
