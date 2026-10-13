package com.elang.reminderwakeonly;

import android.content.Context;
import android.content.SharedPreferences;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

final class LogStore {
    private static final String PREFS = "wake_only_v05";
    private LogStore() { }
    static void add(Context ctx, String msg) {
        SharedPreferences p = ctx.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        String t = new SimpleDateFormat("HH:mm:ss", Locale.getDefault()).format(new Date());
        String log = p.getString("log", "");
        String next = t + "  " + msg + "\n" + log;
        if (next.length() > 10000) next = next.substring(0, 10000);
        p.edit().putString("log", next).commit();
    }
    static String all(Context ctx) {
        return ctx.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
                .getString("log", "Belum ada kejadian.");
    }
    static void clear(Context ctx) {
        ctx.getSharedPreferences(PREFS, Context.MODE_PRIVATE).edit().remove("log").commit();
    }
}
