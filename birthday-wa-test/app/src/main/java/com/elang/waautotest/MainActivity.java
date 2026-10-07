package com.elang.waautotest;

import android.accessibilityservice.AccessibilityServiceInfo;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.PackageManager;
import android.graphics.Color;
import android.net.Uri;
import android.os.Bundle;
import android.provider.Settings;
import android.text.InputType;
import android.view.View;
import android.view.accessibility.AccessibilityManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import android.widget.TextView;
import android.widget.Toast;

import java.util.List;

public class MainActivity extends Activity {
    private static final String PREFS = "auto_send_prefs";
    private static final String WA_PACKAGE = "com.whatsapp";

    private EditText phoneInput;
    private EditText messageInput;
    private TextView statusText;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        ScrollView scroll = new ScrollView(this);
        LinearLayout root = new LinearLayout(this);
        root.setOrientation(LinearLayout.VERTICAL);
        int p = dp(20);
        root.setPadding(p, p, p, p);
        root.setBackgroundColor(Color.WHITE);
        scroll.addView(root);

        TextView title = new TextView(this);
        title.setText("WA Auto Send Test V0.1");
        title.setTextSize(24);
        title.setTextColor(Color.BLACK);
        root.addView(title);

        TextView info = new TextView(this);
        info.setText(
                "\nTest pribadi: WA biasa → nomor WA Business.\n" +
                "1) Isi nomor tujuan format 62812...\n" +
                "2) Aktifkan Accessibility service\n" +
                "3) Tekan Test Send\n\n" +
                "Service hanya armed selama 30 detik setelah Test Send.\n" +
                "Kalau Android memblokir Accessibility karena APK sideload, buka App info → menu ⋮ → Allow restricted settings."
        );
        info.setTextSize(15);
        info.setTextColor(Color.DKGRAY);
        root.addView(info);

        statusText = new TextView(this);
        statusText.setTextSize(16);
        statusText.setPadding(0, dp(16), 0, dp(8));
        root.addView(statusText);

        phoneInput = new EditText(this);
        phoneInput.setHint("Nomor WA Business, contoh: 6281234567890");
        phoneInput.setInputType(InputType.TYPE_CLASS_PHONE);
        root.addView(phoneInput, fullWidth());

        messageInput = new EditText(this);
        messageInput.setHint("Pesan test");
        messageInput.setMinLines(3);
        messageInput.setGravity(android.view.Gravity.TOP);
        messageInput.setText("Test otomatis dari WA pribadi ke WA Business ✅");
        root.addView(messageInput, fullWidth());

        Button accessibilityButton = new Button(this);
        accessibilityButton.setText("1. Aktifkan Accessibility");
        accessibilityButton.setOnClickListener(v -> {
            try {
                startActivity(new Intent(Settings.ACTION_ACCESSIBILITY_SETTINGS));
            } catch (Exception e) {
                Toast.makeText(this, "Tidak bisa membuka Accessibility settings", Toast.LENGTH_LONG).show();
            }
        });
        root.addView(accessibilityButton, fullWidth());

        Button testButton = new Button(this);
        testButton.setText("2. Test Send dari WA Pribadi");
        testButton.setOnClickListener(v -> runTest());
        root.addView(testButton, fullWidth());

        TextView note = new TextView(this);
        note.setText(
                "\nV0.1 sengaja text-only. Kalau ini PASS, berikutnya attachment + teks, lalu scheduler birthday jam 00:00."
        );
        note.setTextSize(14);
        note.setTextColor(Color.DKGRAY);
        root.addView(note);

        setContentView(scroll);
    }

    @Override
    protected void onResume() {
        super.onResume();
        updateAccessibilityStatus();
    }

    private void runTest() {
        String phone = normalizePhone(phoneInput.getText().toString());
        String message = messageInput.getText().toString().trim();

        if (phone.length() < 10) {
            Toast.makeText(this, "Isi nomor tujuan dulu, bos. Format 62812...", Toast.LENGTH_LONG).show();
            return;
        }
        if (message.isEmpty()) {
            Toast.makeText(this, "Pesannya masih kosong.", Toast.LENGTH_SHORT).show();
            return;
        }
        if (!isPackageInstalled(WA_PACKAGE)) {
            Toast.makeText(this, "WhatsApp biasa tidak ditemukan.", Toast.LENGTH_LONG).show();
            return;
        }
        if (!isAccessibilityEnabled()) {
            Toast.makeText(this, "Aktifkan WA Auto Send Test di Accessibility dulu.", Toast.LENGTH_LONG).show();
            try {
                startActivity(new Intent(Settings.ACTION_ACCESSIBILITY_SETTINGS));
            } catch (Exception ignored) {}
            return;
        }

        SharedPreferences prefs = getSharedPreferences(PREFS, MODE_PRIVATE);
        prefs.edit()
                .putLong("armed_until", System.currentTimeMillis() + 30_000L)
                .putString("expected_package", WA_PACKAGE)
                .apply();

        try {
            Uri uri = Uri.parse("https://wa.me/" + phone + "?text=" + Uri.encode(message));
            Intent intent = new Intent(Intent.ACTION_VIEW, uri);
            intent.setPackage(WA_PACKAGE);
            startActivity(intent);
            Toast.makeText(this, "Armed 30 detik. Membuka WhatsApp…", Toast.LENGTH_SHORT).show();
        } catch (Exception e) {
            prefs.edit().putLong("armed_until", 0L).apply();
            Toast.makeText(this, "Gagal membuka WhatsApp: " + e.getMessage(), Toast.LENGTH_LONG).show();
        }
    }

    private String normalizePhone(String raw) {
        String n = raw == null ? "" : raw.replaceAll("[^0-9]", "");
        if (n.startsWith("0")) n = "62" + n.substring(1);
        return n;
    }

    private boolean isPackageInstalled(String pkg) {
        try {
            getPackageManager().getPackageInfo(pkg, 0);
            return true;
        } catch (PackageManager.NameNotFoundException e) {
            return false;
        }
    }

    private void updateAccessibilityStatus() {
        boolean on = isAccessibilityEnabled();
        statusText.setText(on
                ? "Accessibility: ON ✅"
                : "Accessibility: OFF ❌");
        statusText.setTextColor(on ? Color.rgb(0, 120, 60) : Color.rgb(180, 0, 0));
    }

    private boolean isAccessibilityEnabled() {
        AccessibilityManager am =
                (AccessibilityManager) getSystemService(Context.ACCESSIBILITY_SERVICE);
        if (am == null) return false;

        List<AccessibilityServiceInfo> enabled =
                am.getEnabledAccessibilityServiceList(AccessibilityServiceInfo.FEEDBACK_ALL_MASK);

        for (AccessibilityServiceInfo info : enabled) {
            if (info.getResolveInfo() == null || info.getResolveInfo().serviceInfo == null) continue;
            String pkg = info.getResolveInfo().serviceInfo.packageName;
            String cls = info.getResolveInfo().serviceInfo.name;
            if (getPackageName().equals(pkg)
                    && AutoSendAccessibilityService.class.getName().equals(cls)) {
                return true;
            }
        }
        return false;
    }

    private LinearLayout.LayoutParams fullWidth() {
        LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MATCH_PARENT,
                LinearLayout.LayoutParams.WRAP_CONTENT
        );
        lp.setMargins(0, dp(8), 0, dp(8));
        return lp;
    }

    private int dp(int value) {
        return (int) (value * getResources().getDisplayMetrics().density + 0.5f);
    }
}
