package com.elang.reminderwakeonly;

import android.Manifest;
import android.app.Activity;
import android.app.AlarmManager;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.Settings;
import android.view.Gravity;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import android.widget.TextView;
import android.widget.Toast;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

public class MainActivity extends Activity {
    private TextView state, log;
    private static final int REQUEST_NOTIFICATION = 101;
    private static final int ALARM_REQUEST_ID = 3105;

    @Override public void onCreate(Bundle saved) {
        super.onCreate(saved);
        ScrollView scroll = new ScrollView(this);
        LinearLayout root = new LinearLayout(this);
        root.setOrientation(LinearLayout.VERTICAL);
        int p = dp(18);
        root.setPadding(p,p,p,p);
        scroll.addView(root);
        TextView heading = new TextView(this);
        heading.setText("REMINDER V0.5 • WAKE ONLY");
        heading.setTextSize(23);
        heading.setGravity(Gravity.CENTER);
        root.addView(heading, new LinearLayout.LayoutParams(-1,dp(65)));
        TextView help = new TextView(this);
        help.setText("Tes layar nyala otomatis dari mode sleep. Tidak ada WhatsApp, Send, atau Accessibility.");
        help.setTextSize(15);
        root.addView(help);
        state = new TextView(this);
        state.setTextSize(15);
        state.setPadding(0,dp(12),0,dp(12));
        root.addView(state);
        button(root, "1. Cek / atur izin", () -> managePermissions());
        button(root, "2. Jadwalkan tes 60 detik", () -> schedule(60));
        button(root, "Jadwalkan tes 120 detik", () -> schedule(120));
        button(root, "Batalkan alarm", () -> cancelAlarm());
        button(root, "Refresh log", () -> refresh());
        button(root, "Hapus log", () -> { LogStore.clear(this); refresh(); });
        TextView tag = new TextView(this);
        tag.setText("Diagnostic (paling baru di atas)");
        tag.setTextSize(17);
        tag.setPadding(0,dp(18),0,dp(6));
        root.addView(tag);
        log = new TextView(this);
        log.setTextSize(14);
        log.setTextIsSelectable(true);
        root.addView(log, new LinearLayout.LayoutParams(-1,ViewGroup.LayoutParams.WRAP_CONTENT));
        setContentView(scroll);
        LogStore.add(this, "APP_OPENED");
        refresh();
    }

    @Override protected void onResume() {
        super.onResume();
        if (state != null) refresh();
    }

    private void button(LinearLayout root,String title,Runnable action) {
        Button b = new Button(this);
        b.setText(title);
        root.addView(b, new LinearLayout.LayoutParams(-1,dp(56)));
        b.setOnClickListener(v -> action.run());
    }

    private int dp(int x) { return Math.round(x*getResources().getDisplayMetrics().density); }

    private boolean notificationAllowed() {
        return Build.VERSION.SDK_INT<33 ||
            checkSelfPermission(Manifest.permission.POST_NOTIFICATIONS)==PackageManager.PERMISSION_GRANTED;
    }
    private AlarmManager alarm() { return (AlarmManager)getSystemService(Context.ALARM_SERVICE); }
    private NotificationManager notifications() { return (NotificationManager)getSystemService(Context.NOTIFICATION_SERVICE); }
    private boolean exactAllowed() { return Build.VERSION.SDK_INT<31 || alarm().canScheduleExactAlarms(); }
    private boolean fsiAllowed() { return Build.VERSION.SDK_INT<34 || notifications().canUseFullScreenIntent(); }

    private void refresh() {
        state.setText("Notification: " + (notificationAllowed()?"OK":"PERLU IZIN")+
           "\nExact alarm: " + (exactAllowed()?"OK":"PERLU IZIN")+
           "\nFull-screen intent: " + (fsiAllowed()?"OK":"PERLU IZIN"));
        log.setText(LogStore.all(this));
    }

    private void managePermissions() {
        if (!notificationAllowed() && Build.VERSION.SDK_INT >= 33) {
            requestPermissions(new String[]{Manifest.permission.POST_NOTIFICATIONS}, REQUEST_NOTIFICATION);
            return;
        }
        if (!exactAllowed() && Build.VERSION.SDK_INT >= 31) {
            try { startActivity(new Intent(Settings.ACTION_REQUEST_SCHEDULE_EXACT_ALARM)
                    .setData(Uri.parse("package:"+getPackageName()))); }
            catch (Exception e) { LogStore.add(this, "EXACT_PERMISSION_SETTINGS_FAILED: " + e.getClass().getSimpleName()); }
            return;
        }
        if (!fsiAllowed() && Build.VERSION.SDK_INT >= 34) {
            try { startActivity(new Intent(Settings.ACTION_MANAGE_APP_USE_FULL_SCREEN_INTENT)
                    .setData(Uri.parse("package:"+getPackageName()))); }
            catch (Exception e) { LogStore.add(this, "FSI_PERMISSION_SETTINGS_FAILED: " + e.getClass().getSimpleName()); }
            return;
        }
        Toast.makeText(this,"Semua izin tes sudah OK",Toast.LENGTH_LONG).show();
        refresh();
    }

    private PendingIntent alarmIntent(int flags) {
        return PendingIntent.getBroadcast(this, ALARM_REQUEST_ID,
                new Intent(this, WakeReceiver.class), flags | PendingIntent.FLAG_IMMUTABLE);
    }
    private void schedule(int seconds) {
        if (!notificationAllowed() || !exactAllowed() || !fsiAllowed()) {
            LogStore.add(this, "PERMISSION_MISSING: tes belum dijadwalkan");
            refresh();
            Toast.makeText(this,"Aktifkan semua izin dahulu",Toast.LENGTH_LONG).show();
            return;
        }
        long at = System.currentTimeMillis() + seconds*1000L;
        PendingIntent fire = alarmIntent(PendingIntent.FLAG_UPDATE_CURRENT);
        PendingIntent display = PendingIntent.getActivity(this, ALARM_REQUEST_ID,
                new Intent(this, MainActivity.class),
                PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);
        try {
            alarm().setAlarmClock(new AlarmManager.AlarmClockInfo(at,display),fire);
            String h = new SimpleDateFormat("HH:mm:ss",Locale.getDefault()).format(new Date(at));
            LogStore.add(this,"SCHEDULED:"+h+" ("+seconds+" sec)");
            Toast.makeText(this,"Tes terjadwal " + h,Toast.LENGTH_LONG).show();
        } catch (Exception e) {
            LogStore.add(this,"ALARM_SCHEDULE_FAILED:"+e.getClass().getSimpleName());
        }
        refresh();
    }
    private void cancelAlarm() {
        PendingIntent pi=alarmIntent(PendingIntent.FLAG_NO_CREATE);
        if (pi!=null) { alarm().cancel(pi); pi.cancel(); }
        LogStore.add(this,"ALARM_CANCELLED");
        refresh();
    }
}
