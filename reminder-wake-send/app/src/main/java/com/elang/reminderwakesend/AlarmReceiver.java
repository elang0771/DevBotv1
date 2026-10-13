package com.elang.reminderwakesend;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.PowerManager;

public class AlarmReceiver extends BroadcastReceiver {
    private static final String CHANNEL="wake_send_test_v06_alarm";
    @Override public void onReceive(Context c, Intent source) {
        Diag.log(c,"ALARM_FIRED");
        PowerManager power=(PowerManager)c.getSystemService(Context.POWER_SERVICE);
        Diag.log(c,"SCREEN_INTERACTIVE_AT_ALARM:"+(power!=null&&power.isInteractive()));
        String phone=source.getStringExtra("phone"),caption=source.getStringExtra("caption"),
            photo=source.getStringExtra("photo"),mime=source.getStringExtra("mime");
        if(phone==null||caption==null||photo==null){
            Diag.log(c,"ALARM_DATA_MISSING");return;
        }
        NotificationManager nm=(NotificationManager)c.getSystemService(Context.NOTIFICATION_SERVICE);
        NotificationChannel channel=new NotificationChannel(CHANNEL,
            "Wake WhatsApp Test",NotificationManager.IMPORTANCE_HIGH);
        channel.setLockscreenVisibility(Notification.VISIBILITY_PUBLIC);
        nm.createNotificationChannel(channel);
        Intent wake=new Intent(c,WakeDispatchActivity.class)
            .addFlags(Intent.FLAG_ACTIVITY_NEW_TASK|Intent.FLAG_ACTIVITY_CLEAR_TOP)
            .putExtra("phone",phone).putExtra("caption",caption)
            .putExtra("photo",photo).putExtra("mime",mime);
        PendingIntent full=PendingIntent.getActivity(c,6006,wake,
            PendingIntent.FLAG_UPDATE_CURRENT|PendingIntent.FLAG_IMMUTABLE);
        Notification n=new Notification.Builder(c,CHANNEL)
            .setSmallIcon(android.R.drawable.ic_lock_idle_alarm)
            .setContentTitle("Reminder Auto Send Test V0.6")
            .setContentText("Alarm aktif — membangunkan layar")
            .setCategory(Notification.CATEGORY_ALARM)
            .setPriority(Notification.PRIORITY_MAX)
            .setVisibility(Notification.VISIBILITY_PUBLIC)
            .setContentIntent(full)
            .setFullScreenIntent(full,true)
            .setAutoCancel(true).build();
        try {nm.notify(6006,n);Diag.log(c,"WAKE_FULL_SCREEN_REQUESTED");}
        catch(Exception e){Diag.log(c,"WAKE_NOTIFICATION_ERROR:"+e.getClass().getSimpleName());}
    }
}
