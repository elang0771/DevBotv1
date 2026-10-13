package com.elang.reminderwakesend;

import android.Manifest;
import android.accessibilityservice.AccessibilityServiceInfo;
import android.app.Activity;
import android.app.AlarmManager;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.database.Cursor;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.OpenableColumns;
import android.provider.Settings;
import android.text.InputType;
import android.view.accessibility.AccessibilityManager;
import android.widget.*;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;
import java.util.Locale;

public class MainActivity extends Activity {
    static final String WA="com.whatsapp";
    private static final int REQ_PHOTO=901, REQ_NOTIF=902;
    private EditText phone, caption, seconds;
    private TextView state, diag, picked;
    private Uri photo;

    @Override public void onCreate(Bundle b) {
        super.onCreate(b);
        ScrollView scroll=new ScrollView(this);
        LinearLayout root=new LinearLayout(this);
        root.setOrientation(LinearLayout.VERTICAL);
        int p=dp(17);root.setPadding(p,p,p,p);scroll.addView(root);
        TextView title=new TextView(this);
        title.setText("REMINDER V0.9 • WAKE + WA SEND TEST");title.setTextSize(23);
        root.addView(title);
        TextView intro=new TextView(this);
        intro.setText("TES TERPISAH: layar mati → alarm → layar bangun → buka WhatsApp → tunggu caption → auto tap Send.\nUji hanya ke nomor sendiri terlebih dahulu. Jangan sentuh HP selama tes.");
        intro.setTextSize(15);root.addView(intro);
        state=new TextView(this);state.setTextSize(15);state.setPadding(0,dp(12),0,dp(12));root.addView(state);
        phone=new EditText(this);
        phone.setHint("Nomor penerima (62812...)");
        phone.setInputType(InputType.TYPE_CLASS_PHONE);
        phone.setSingleLine(true);
        root.addView(phone);
        caption=new EditText(this);
        caption.setText("Test Reminder WakeSend V0.6 ✅");
        caption.setHint("Caption foto");
        caption.setMinLines(2);
        root.addView(caption);
        seconds=new EditText(this);
        seconds.setInputType(InputType.TYPE_CLASS_NUMBER);
        seconds.setText("90");
        seconds.setHint("Delay detik 60–600");
        root.addView(seconds);
        button(root,"1. PILIH FOTO",()->pick());
        picked=new TextView(this);
        picked.setText("Attachment: belum dipilih");
        picked.setTextSize(14);
        picked.setPadding(0,dp(3),0,dp(8));root.addView(picked);
        button(root,"2. CEK / ATUR IZIN",()->permissions());
        button(root,"3. BUKA PENGATURAN ACCESSIBILITY",()->{
            try {startActivity(new Intent(Settings.ACTION_ACCESSIBILITY_SETTINGS));}
            catch(Exception e){Diag.log(this,"ACCESSIBILITY_SETTINGS_ERROR:"+e.getClass().getSimpleName());}
        });
        button(root,"4. JADWALKAN TEST AUTO SEND",()->schedule());
        button(root,"BATALKAN / HENTIKAN AUTO SEND",()->cancel());
        button(root,"REFRESH DIAGNOSTIC",()->refresh());
        button(root,"HAPUS DIAGNOSTIC",()->{Diag.clear(this);refresh();});
        TextView label=new TextView(this);label.setText("DIAGNOSTIC LOG — yang terbaru di atas");
        label.setTextSize(17);label.setPadding(0,dp(12),0,0);root.addView(label);
        diag=new TextView(this);diag.setTextSize(14);diag.setTextIsSelectable(true);root.addView(diag);
        setContentView(scroll);

        String oldPhoto=Diag.prefs(this).getString("photo",null);
        if(oldPhoto!=null) {
            photo=Uri.parse(oldPhoto);
            picked.setText("Attachment tersimpan: "+photo.getLastPathSegment());
        }
        phone.setText(Diag.prefs(this).getString("phone",""));
        caption.setText(Diag.prefs(this).getString("caption","Test Reminder WakeSend V0.6 ✅"));
        refresh();
    }
    @Override public void onResume() {super.onResume(); if(state!=null) refresh();}
    int dp(int x){return Math.round(x*getResources().getDisplayMetrics().density);}
    void button(LinearLayout root,String name,Runnable action){
        Button b=new Button(this);b.setText(name);
        LinearLayout.LayoutParams lp=new LinearLayout.LayoutParams(-1,dp(54));lp.topMargin=dp(4);
        root.addView(b,lp);b.setOnClickListener(v->action.run());
    }
    void pick(){
        Intent i=new Intent(Intent.ACTION_OPEN_DOCUMENT);
        i.addCategory(Intent.CATEGORY_OPENABLE);
        i.setType("image/*");
        startActivityForResult(i,REQ_PHOTO);
    }
    @Override protected void onActivityResult(int req,int result,Intent data){
        super.onActivityResult(req,result,data);
        if(req!=REQ_PHOTO||result!=RESULT_OK||data==null||data.getData()==null)return;
        photo=data.getData();
        try {getContentResolver().takePersistableUriPermission(photo,Intent.FLAG_GRANT_READ_URI_PERMISSION);}
        catch(Exception e){Diag.log(this,"PHOTO_PERMISSION_WARNING:"+e.getClass().getSimpleName());}
        String name=photo.getLastPathSegment();
        try(Cursor cur=getContentResolver().query(photo,null,null,null,null)){
            if(cur!=null&&cur.moveToFirst()) {
                int ix=cur.getColumnIndex(OpenableColumns.DISPLAY_NAME);
                if(ix>=0)name=cur.getString(ix);
            }
        }catch(Exception ignored){}
        picked.setText("Attachment: "+name);
        Diag.prefs(this).edit().putString("photo",photo.toString()).apply();
    }
    boolean notifOk(){return Build.VERSION.SDK_INT<33||checkSelfPermission(Manifest.permission.POST_NOTIFICATIONS)==PackageManager.PERMISSION_GRANTED;}
    boolean exactOk(){return Build.VERSION.SDK_INT<31||((AlarmManager)getSystemService(ALARM_SERVICE)).canScheduleExactAlarms();}
    boolean fullOk(){return Build.VERSION.SDK_INT<34||((NotificationManager)getSystemService(NOTIFICATION_SERVICE)).canUseFullScreenIntent();}
    boolean accessOk(){
        AccessibilityManager manager=(AccessibilityManager)getSystemService(ACCESSIBILITY_SERVICE);
        if(manager==null)return false;
        List<AccessibilityServiceInfo> list=manager.getEnabledAccessibilityServiceList(AccessibilityServiceInfo.FEEDBACK_ALL_MASK);
        for(AccessibilityServiceInfo info:list){
            if(info.getResolveInfo()==null || info.getResolveInfo().serviceInfo==null)continue;
            if(getPackageName().equals(info.getResolveInfo().serviceInfo.packageName) &&
                    AutoSendAccessibilityService.class.getName().equals(info.getResolveInfo().serviceInfo.name))return true;
        }
        return false;
    }
    boolean waOk(){try {getPackageManager().getPackageInfo(WA,0);return true;}
        catch(PackageManager.NameNotFoundException e){return false;}}
    void refresh() {
        state.setText("Notification: "+(notifOk()?"OK":"IZINKAN")+
            "\nExact Alarm: "+(exactOk()?"OK":"IZINKAN")+
            "\nFull-screen Intent: "+(fullOk()?"OK":"IZINKAN")+
            "\nAccessibility V0.6: "+(accessOk()?"ON":"OFF")+
            "\nWhatsApp biasa: "+(waOk()?"ADA":"BELUM TERDETEKSI")+
            "\nStatus: "+Diag.prefs(this).getString("state","belum test"));
        diag.setText(Diag.all(this));
    }
    void permissions(){
        if(!notifOk() && Build.VERSION.SDK_INT>=33) {
            requestPermissions(new String[]{Manifest.permission.POST_NOTIFICATIONS},REQ_NOTIF);return;
        }
        if(!exactOk() && Build.VERSION.SDK_INT>=31){
            startActivity(new Intent(Settings.ACTION_REQUEST_SCHEDULE_EXACT_ALARM).setData(Uri.parse("package:"+getPackageName())));return;
        }
        if(!fullOk() && Build.VERSION.SDK_INT>=34){
            startActivity(new Intent(Settings.ACTION_MANAGE_APP_USE_FULL_SCREEN_INTENT).setData(Uri.parse("package:"+getPackageName())));return;
        }
        Toast.makeText(this,"Izin alarm OK. Accessibility diatur lewat tombol nomor 3.",Toast.LENGTH_LONG).show();
    }
    String normalize(String s){
        String n=s.replaceAll("[^0-9]","");
        if(n.startsWith("0"))n="62"+n.substring(1);
        return n;
    }
    void schedule(){
        String n=normalize(phone.getText().toString()), c=caption.getText().toString().trim();
        if(n.length()<10 || n.length()>16 || c.isEmpty() || photo==null){
            Toast.makeText(this,"Isi nomor, caption dan pilih foto dulu.",Toast.LENGTH_LONG).show();return;
        }
        if(!notifOk() || !exactOk() || !fullOk() || !accessOk() || !waOk()) {
            Toast.makeText(this,"Masih ada izin atau WhatsApp yang belum siap.",Toast.LENGTH_LONG).show();refresh();return;
        }
        int delay;
        try {delay=Integer.parseInt(seconds.getText().toString());}catch(Exception e){delay=90;}
        delay=Math.max(60,Math.min(600,delay));
        String mime=getContentResolver().getType(photo);
        if(mime==null||!mime.startsWith("image/"))mime="image/jpeg";
        long at=System.currentTimeMillis()+delay*1000L;
        // One active scheduled job in this isolated test, never queues duplicate sends.
        PendingIntent existing=alarmPi(PendingIntent.FLAG_NO_CREATE);
        if(existing!=null){((AlarmManager)getSystemService(ALARM_SERVICE)).cancel(existing);existing.cancel();}
        Diag.prefs(this).edit().putLong("armed_until",0L).putBoolean("dispatch_started",false).putString("phone",n)
            .putString("caption",c).putString("photo",photo.toString())
            .putString("state","SCHEDULED").commit();
        Intent i=new Intent(this,AlarmReceiver.class);
        i.putExtra("phone",n);
        i.putExtra("caption",c);
        i.putExtra("photo",photo.toString());
        i.putExtra("mime",mime);
        PendingIntent alarmPi=PendingIntent.getBroadcast(this,3006,i,
            PendingIntent.FLAG_UPDATE_CURRENT|PendingIntent.FLAG_IMMUTABLE);
        PendingIntent showPi=PendingIntent.getActivity(this,3008,
            new Intent(this,MainActivity.class),
            PendingIntent.FLAG_UPDATE_CURRENT|PendingIntent.FLAG_IMMUTABLE);
        try {
            ((AlarmManager)getSystemService(ALARM_SERVICE)).setAlarmClock(new AlarmManager.AlarmClockInfo(at,showPi),alarmPi);
            String t=new SimpleDateFormat("HH:mm:ss",Locale.getDefault()).format(new Date(at));
            Diag.log(this,"SCHEDULED_FOR:"+t+" ("+delay+" sec)");
            Toast.makeText(this,"Jadwal test: "+t+" — matikan layar",Toast.LENGTH_LONG).show();
        }catch(Exception e){Diag.log(this,"SCHEDULE_FAILED:"+e.getClass().getSimpleName());}
        refresh();
    }
    PendingIntent alarmPi(int flags){return PendingIntent.getBroadcast(this,3006,
        new Intent(this,AlarmReceiver.class),flags|PendingIntent.FLAG_IMMUTABLE);}
    void cancel(){
        PendingIntent pi=alarmPi(PendingIntent.FLAG_NO_CREATE);
        if(pi!=null){((AlarmManager)getSystemService(ALARM_SERVICE)).cancel(pi);pi.cancel();}
        Diag.disarm(this,"CANCELLED_BY_USER");
        refresh();
    }
}
