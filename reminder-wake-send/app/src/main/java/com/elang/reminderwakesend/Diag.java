package com.elang.reminderwakesend;

import android.content.Context;
import android.content.SharedPreferences;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

final class Diag {
    static final String PREF="reminder_wake_send_v06";
    private Diag() {}
    static SharedPreferences prefs(Context c) { return c.getSharedPreferences(PREF,Context.MODE_PRIVATE); }
    static synchronized void log(Context c,String detail) {
        SharedPreferences p=prefs(c);
        String now=new SimpleDateFormat("HH:mm:ss",Locale.getDefault()).format(new Date());
        String entry=now+"  "+detail+"\n"+p.getString("diag","");
        if(entry.length()>14000) entry=entry.substring(0,14000);
        p.edit().putString("diag",entry).commit();
    }
    static String all(Context c) {return prefs(c).getString("diag","Belum ada proses.");}
    static void clear(Context c) {prefs(c).edit().remove("diag").apply();}
    static void disarm(Context c, String status) {
        prefs(c).edit().putLong("armed_until",0).putString("state",status).commit();
        log(c,status);
    }
}
