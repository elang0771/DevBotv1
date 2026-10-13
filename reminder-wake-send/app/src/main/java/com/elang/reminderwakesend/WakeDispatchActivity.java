package com.elang.reminderwakesend;

import android.app.Activity;
import android.content.ClipData;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.PowerManager;
import android.content.Context;
import android.graphics.Color;
import android.view.Gravity;
import android.view.WindowManager;
import android.widget.LinearLayout;
import android.widget.TextView;

public class WakeDispatchActivity extends Activity {
    private boolean started;
    @Override public void onCreate(Bundle state){
        super.onCreate(state);
        setShowWhenLocked(true);
        setTurnScreenOn(true);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        LinearLayout root=new LinearLayout(this);
        root.setBackgroundColor(Color.rgb(7,123,77));
        root.setGravity(Gravity.CENTER);
        TextView message=new TextView(this);
        message.setGravity(Gravity.CENTER);
        message.setTextColor(Color.WHITE);
        message.setTextSize(24);
        message.setText("LAYAR BANGUN ✓\nMempersiapkan WhatsApp…");
        root.addView(message);
        setContentView(root);
        Diag.log(this,"WAKE_ACTIVITY_CREATED");
    }
    @Override public void onResume(){
        super.onResume();
        PowerManager pm=(PowerManager)getSystemService(Context.POWER_SERVICE);
        Diag.log(this,"WAKE_ACTIVITY_RESUMED_SCREEN_INTERACTIVE:"+(pm!=null&&pm.isInteractive()));
        if(!started){
            started=true;
            new Handler(Looper.getMainLooper()).postDelayed(this::openWhatsApp,1150);
        }
    }
    @Override public void onWindowFocusChanged(boolean focus){
        super.onWindowFocusChanged(focus);
        if(focus)Diag.log(this,"WAKE_WINDOW_FOCUSED");
    }
    private void openWhatsApp(){
        String phone=getIntent().getStringExtra("phone"),
            caption=getIntent().getStringExtra("caption"),
            photo=getIntent().getStringExtra("photo"),
            mime=getIntent().getStringExtra("mime");
        if(phone==null||caption==null||photo==null) {Diag.log(this,"DISPATCH_MISSING_DATA");finish();return;}
        if(mime==null||!mime.startsWith("image/"))mime="image/jpeg";
        Uri uri=Uri.parse(photo);
        Diag.prefs(this).edit()
            .putString("expected_caption",caption)
            .putString("expected_phone",phone)
            .putLong("armed_until",System.currentTimeMillis()+95000L)
            .putBoolean("click_in_progress",false)
            .putString("state","WAITING_WHATSAPP_PREVIEW").commit();
        Intent share=new Intent(Intent.ACTION_SEND);
        share.setType(mime);
        share.setPackage(MainActivity.WA);
        share.putExtra(Intent.EXTRA_STREAM,uri);
        share.putExtra(Intent.EXTRA_TEXT,caption);
        share.putExtra("jid",phone+"@s.whatsapp.net");
        share.setClipData(ClipData.newRawUri("attachment",uri));
        share.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
        Diag.log(this,"WHATSAPP_LAUNCH_REQUESTED_AFTER_WAKE");
        try {
            startActivity(share);
            Diag.log(this,"WHATSAPP_START_ACTIVITY_ACCEPTED");
        }catch(Exception e){
            Diag.disarm(this,"WHATSAPP_START_FAILED:"+e.getClass().getSimpleName());
        }
    }
}
