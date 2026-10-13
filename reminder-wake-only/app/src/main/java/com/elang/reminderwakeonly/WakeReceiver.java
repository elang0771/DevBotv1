package com.elang.reminderwakeonly;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.os.PowerManager;

public class WakeReceiver extends BroadcastReceiver {
    private static final String CHANNEL = "wake_test_alarm_v05";
    private static final int NOTIF_ID = 501;
    @Override public void onReceive(Context context, Intent ignored) {
        LogStore.add(context,"ALARM_FIRED");
        PowerManager pm = (PowerManager) context.getSystemService(Context.POWER_SERVICE);
        LogStore.add(context,"SCREEN_INTERACTIVE_AT_ALARM:"+(pm!=null && pm.isInteractive()));
        NotificationManager manager = (NotificationManager)
                context.getSystemService(Context.NOTIFICATION_SERVICE);
        try {
            NotificationChannel channel = new NotificationChannel(CHANNEL,
                    "Wake Test Alarm", NotificationManager.IMPORTANCE_HIGH);
            channel.setDescription("Scheduled one-shot screen wake test");
            channel.setLockscreenVisibility(Notification.VISIBILITY_PUBLIC);
            manager.createNotificationChannel(channel);
            Intent wake = new Intent(context,WakeActivity.class)
                    .addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP);
            PendingIntent fullscreen = PendingIntent.getActivity(context, NOTIF_ID, wake,
                    PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);
            Notification notification = new Notification.Builder(context,CHANNEL)
                    .setSmallIcon(android.R.drawable.ic_lock_idle_alarm)
                    .setContentTitle("Reminder Wake Test: ALARM")
                    .setContentText("Tes layar bangun otomatis. Tidak kirim WhatsApp.")
                    .setCategory(Notification.CATEGORY_ALARM)
                    .setPriority(Notification.PRIORITY_MAX)
                    .setVisibility(Notification.VISIBILITY_PUBLIC)
                    .setOngoing(false)
                    .setAutoCancel(true)
                    .setContentIntent(fullscreen)
                    .setFullScreenIntent(fullscreen,true)
                    .build();
            manager.notify(NOTIF_ID,notification);
            LogStore.add(context,"FULL_SCREEN_NOTIFICATION_POSTED");
            if (Build.VERSION.SDK_INT >= 34 && !manager.canUseFullScreenIntent())
                LogStore.add(context,"WARNING_FULL_SCREEN_PERMISSION_DENIED");
        } catch (Exception e) {
            LogStore.add(context,"NOTIFICATION_FAILED:"+e.getClass().getSimpleName());
        }
    }
}
