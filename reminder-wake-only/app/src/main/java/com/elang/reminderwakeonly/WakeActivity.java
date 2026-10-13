package com.elang.reminderwakeonly;

import android.app.Activity;
import android.graphics.Color;
import android.os.Bundle;
import android.os.PowerManager;
import android.content.Context;
import android.view.Gravity;
import android.view.WindowManager;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Button;

public class WakeActivity extends Activity {
    @Override public void onCreate(Bundle state) {
        super.onCreate(state);
        setShowWhenLocked(true);
        setTurnScreenOn(true);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        LogStore.add(this,"WAKE_ACTIVITY_CREATED");
        LinearLayout root = new LinearLayout(this);
        root.setPadding(32,55,32,32);
        root.setGravity(Gravity.CENTER);
        root.setBackgroundColor(Color.rgb(5,135,72));
        root.setOrientation(LinearLayout.VERTICAL);
        TextView title = new TextView(this);
        title.setGravity(Gravity.CENTER);
        title.setText("LAYAR BERHASIL DIBANGUNKAN");
        title.setTextColor(Color.WHITE);
        title.setTextSize(27);
        root.addView(title);
        TextView subtitle = new TextView(this);
        subtitle.setGravity(Gravity.CENTER);
        subtitle.setText("Reminder Wake Only V0.5\nTidak ada WhatsApp yang dikirim");
        subtitle.setTextSize(17);
        subtitle.setTextColor(Color.WHITE);
        root.addView(subtitle);
        Button button = new Button(this);
        button.setText("Selesai, kembali ke diagnostic");
        button.setOnClickListener(v->{
            startActivity(new android.content.Intent(this,MainActivity.class));
            finish();
        });
        root.addView(button);
        setContentView(root);
    }
    @Override protected void onResume() {
        super.onResume();
        PowerManager pm=(PowerManager)getSystemService(Context.POWER_SERVICE);
        LogStore.add(this,"WAKE_ACTIVITY_RESUMED_INTERACTIVE:"+(pm!=null && pm.isInteractive()));
    }
    @Override public void onWindowFocusChanged(boolean focused) {
        super.onWindowFocusChanged(focused);
        if (focused) LogStore.add(this,"WAKE_WINDOW_FOCUSED");
    }
}
