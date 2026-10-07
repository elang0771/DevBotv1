package com.elang.waautotest;

import android.accessibilityservice.AccessibilityService;
import android.content.SharedPreferences;
import android.os.Handler;
import android.os.Looper;
import android.view.accessibility.AccessibilityEvent;
import android.view.accessibility.AccessibilityNodeInfo;
import android.widget.Toast;

import java.util.List;

public class AutoSendAccessibilityService extends AccessibilityService {
    private static final String PREFS = "auto_send_prefs";
    private static final String WA_PACKAGE = "com.whatsapp";

    private final Handler handler = new Handler(Looper.getMainLooper());
    private boolean attemptPending = false;

    @Override
    public void onAccessibilityEvent(AccessibilityEvent event) {
        if (event == null || event.getPackageName() == null) return;
        if (!WA_PACKAGE.contentEquals(event.getPackageName())) return;

        SharedPreferences prefs = getSharedPreferences(PREFS, MODE_PRIVATE);
        long armedUntil = prefs.getLong("armed_until", 0L);
        if (armedUntil <= System.currentTimeMillis()) return;

        if (attemptPending) return;
        attemptPending = true;

        handler.postDelayed(() -> {
            try {
                tryClickSend();
            } finally {
                attemptPending = false;
            }
        }, 700L);
    }

    private void tryClickSend() {
        SharedPreferences prefs = getSharedPreferences(PREFS, MODE_PRIVATE);
        long armedUntil = prefs.getLong("armed_until", 0L);
        if (armedUntil <= System.currentTimeMillis()) return;

        AccessibilityNodeInfo root = getRootInActiveWindow();
        if (root == null) return;

        AccessibilityNodeInfo target = null;

        try {
            List<AccessibilityNodeInfo> byId =
                    root.findAccessibilityNodeInfosByViewId("com.whatsapp:id/send");
            if (byId != null) {
                for (AccessibilityNodeInfo node : byId) {
                    if (node != null && node.isVisibleToUser()) {
                        target = node;
                        break;
                    }
                }
            }
        } catch (Exception ignored) {}

        if (target == null) {
            target = findSendNode(root);
        }

        if (target != null && clickNodeOrParent(target)) {
            prefs.edit().putLong("armed_until", 0L).apply();
            Toast.makeText(this, "AUTO SEND: tombol Send diklik ✅", Toast.LENGTH_LONG).show();
        }
    }

    private AccessibilityNodeInfo findSendNode(AccessibilityNodeInfo node) {
        if (node == null) return null;

        String id = safe(node.getViewIdResourceName());
        String desc = safe(node.getContentDescription());
        String text = safe(node.getText());

        boolean looksLikeSend =
                id.endsWith(":id/send")
                || id.endsWith("/send")
                || "send".equalsIgnoreCase(desc)
                || "kirim".equalsIgnoreCase(desc)
                || "send".equalsIgnoreCase(text)
                || "kirim".equalsIgnoreCase(text);

        if (looksLikeSend && node.isVisibleToUser()) return node;

        for (int i = 0; i < node.getChildCount(); i++) {
            AccessibilityNodeInfo found = findSendNode(node.getChild(i));
            if (found != null) return found;
        }
        return null;
    }

    private boolean clickNodeOrParent(AccessibilityNodeInfo node) {
        AccessibilityNodeInfo current = node;
        for (int i = 0; i < 4 && current != null; i++) {
            if (current.isClickable()) {
                return current.performAction(AccessibilityNodeInfo.ACTION_CLICK);
            }
            current = current.getParent();
        }
        return node.performAction(AccessibilityNodeInfo.ACTION_CLICK);
    }

    private String safe(CharSequence s) {
        return s == null ? "" : s.toString().trim();
    }

    @Override
    public void onInterrupt() {
    }
}
