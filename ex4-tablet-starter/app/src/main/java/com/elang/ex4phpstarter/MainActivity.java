package com.elang.ex4phpstarter;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.graphics.Typeface;
import android.graphics.drawable.GradientDrawable;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.text.InputType;
import android.util.Base64;
import android.view.Gravity;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import android.widget.TextView;

import com.jcraft.jsch.ChannelExec;
import com.jcraft.jsch.HostKey;
import com.jcraft.jsch.HostKeyRepository;
import com.jcraft.jsch.JSch;
import com.jcraft.jsch.Session;
import com.jcraft.jsch.UserInfo;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.security.MessageDigest;
import java.util.Locale;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

public final class MainActivity extends Activity {
  private static final String HOST = "192.168.18.22";
  private static final String HTTP = "http://" + HOST + ":8090/dashboard.php";
  private static final String BOOT = "/usr/local/config/ex4-webapps-internal/start.sh";
  private static final String CMD = "test -f '" + BOOT + "' || { echo 'Boot EX4 tidak ditemukan'; exit 24; }; sh '" + BOOT + "'";
  private static final String PREF = "trusted_ssh_host_key";
  private static final int TEXT = Color.rgb(31,44,62);
  private static final int MUTED = Color.rgb(98,111,131);
  private static final int PRIMARY = Color.rgb(23,93,190);
  private static final int BACKGROUND = Color.rgb(245,247,251);

  private final Handler ui = new Handler(Looper.getMainLooper());
  private TextView status, details;
  private EditText password;
  private Button startButton, checkButton, openButton;
  private volatile boolean busy = false;

  @Override public void onCreate(Bundle savedState) {
    super.onCreate(savedState);
    buildUi();
    checkStatus();
  }

  private GradientDrawable bg(int color, int radius) {
    GradientDrawable d = new GradientDrawable();
    d.setColor(color); d.setCornerRadius(dp(radius));
    return d;
  }
  private int dp(int value) {
    return Math.round(value * getResources().getDisplayMetrics().density);
  }
  private TextView text(String message, int size, int color, boolean bold) {
    TextView t = new TextView(this);
    t.setText(message); t.setTextColor(color); t.setTextSize(size);
    if (bold) t.setTypeface(Typeface.DEFAULT, Typeface.BOLD);
    return t;
  }
  private void add(LinearLayout layout, View child, int top) {
    LinearLayout.LayoutParams p = new LinearLayout.LayoutParams(-1, -2);
    p.topMargin = dp(top); layout.addView(child,p);
  }
  private Button button(String title, int color) {
    Button b = new Button(this);
    b.setAllCaps(false);
    b.setText(title); b.setTextSize(16); b.setTextColor(Color.WHITE);
    b.setTypeface(Typeface.DEFAULT, Typeface.BOLD);
    b.setBackground(bg(color,13));
    b.setMinHeight(dp(56));
    return b;
  }
  private void buildUi() {
    ScrollView scroll = new ScrollView(this);
    scroll.setFillViewport(true);
    scroll.setBackgroundColor(BACKGROUND);
    LinearLayout root = new LinearLayout(this);
    root.setOrientation(LinearLayout.VERTICAL); root.setPadding(dp(22),dp(28),dp(22),dp(24));
    scroll.addView(root);
    setContentView(scroll);

    TextView title = text("WD My Cloud EX4",24,TEXT,true);
    add(root,title,4);
    add(root,text("PHP Starter  •  Tablet Android",14,MUTED,false),4);

    LinearLayout header = new LinearLayout(this);
    header.setPadding(dp(20),dp(20),dp(20),dp(20));
    header.setOrientation(LinearLayout.VERTICAL);
    GradientDrawable blue = new GradientDrawable(GradientDrawable.Orientation.LEFT_RIGHT,
      new int[]{Color.rgb(24,67,139), Color.rgb(35,117,190)});
    blue.setCornerRadius(dp(18));
    header.setBackground(blue);
    TextView head = text("Jalankan PHP 8090",22,Color.WHITE,true);
    header.addView(head);
    TextView sub = text("Satu tombol untuk menyalakan server EX4. Tidak perlu ConnectBot atau komputer.",14,Color.WHITE,false);
    LinearLayout.LayoutParams hp = new LinearLayout.LayoutParams(-1,-2);hp.topMargin=dp(10);
    header.addView(sub,hp);
    add(root,header,25);

    LinearLayout card = new LinearLayout(this);
    card.setOrientation(LinearLayout.VERTICAL);
    card.setPadding(dp(20),dp(20),dp(20),dp(22));
    card.setBackground(bg(Color.WHITE,18));
    add(card,text("Status PHP 8090",15,MUTED,false),0);
    status = text("Memeriksa koneksi...",21,TEXT,true);
    add(card,status,7);
    details = text("EX4  •  " + HOST + "  •  LAN",13,MUTED,false);
    add(card,details,8);
    add(root,card,17);

    add(root,text("Password SSH root EX4",15,TEXT,true),21);
    password = new EditText(this);
    password.setSingleLine(true);
    password.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
    password.setHint("Isi hanya saat ingin menjalankan");
    password.setTextSize(15);
    password.setPadding(dp(15),dp(10),dp(15),dp(10));
    password.setBackground(bg(Color.WHITE,11));
    add(root,password,9);
    add(root,text("Password tidak disimpan. Saat PHP sudah aktif, tombol status dan Dashboard tidak perlu password.",12,MUTED,false),7);

    startButton = button("▶  START PHP 8090",PRIMARY);
    add(root,startButton,23);
    startButton.setOnClickListener(v -> start());

    checkButton = button("⟳  Cek Status",Color.rgb(78,95,119));
    add(root,checkButton,11);
    checkButton.setOnClickListener(v -> checkStatus());

    openButton = button("↗  Buka Dashboard",Color.rgb(18,128,94));
    add(root,openButton,11);
    openButton.setOnClickListener(v -> {
      try { startActivity(new Intent(Intent.ACTION_VIEW, Uri.parse(HTTP))); }
      catch (Exception e) { setStatus("Browser tidak tersedia", e.getMessage()); }
    });
    add(root,text("Hanya untuk Wi-Fi/LAN EX4. Tidak menginstal PHP baru, tidak mengganti router, dan tidak membuka akses internet publik.",12,MUTED,false),22);
  }

  private void setStatus(String headline, String message) {
    ui.post(() -> {
      status.setText(headline);
      details.setText(message == null ? "" : message);
    });
  }
  private void working(boolean running) {
    busy = running;
    ui.post(() -> {
      startButton.setEnabled(!running);
      checkButton.setEnabled(!running);
      startButton.setAlpha(running ? 0.65f : 1f);
      checkButton.setAlpha(running ? 0.65f : 1f);
    });
  }
  private boolean responds() {
    HttpURLConnection con = null;
    try {
      con = (HttpURLConnection)new URL(HTTP).openConnection();
      con.setConnectTimeout(3000); con.setReadTimeout(3000);
      con.setRequestMethod("GET"); con.setInstanceFollowRedirects(false);
      int code = con.getResponseCode();
      return code >= 200 && code < 500;
    } catch(Exception e) {return false;}
    finally { if (con!=null) con.disconnect(); }
  }
  private void checkStatus() {
    if (busy) return;
    working(true);
    setStatus("Memeriksa koneksi...", "Menghubungi " + HOST + ":8090");
    new Thread(() -> {
      boolean online=responds();
      setStatus(online ? "PHP AKTIF ✓" : "PHP belum terhubung",
        online ? "Dashboard siap dibuka." : "Setelah NAS hidup, tekan START PHP.");
      working(false);
    }, "status-check").start();
  }

  private void start() {
    if (busy) return;
    final String pass = password.getText().toString();
    password.setText("");
    working(true);
    setStatus("Menghubungi EX4...", "Memeriksa apakah PHP sudah berjalan.");
    new Thread(() -> {
      try {
        if (responds()) {
          setStatus("PHP SUDAH AKTIF ✓", "Tidak menjalankan server kedua.");
          return;
        }
        if (pass.length()==0) {
          setStatus("Perlu password SSH", "Isi password SSH root, lalu tekan START PHP.");
          return;
        }
        setStatus("Membuka SSH...", "Koneksi LAN ke " + HOST + ":22");
        String output = execute(pass);
        setStatus("Memeriksa PHP...", "Boot dipanggil. Menunggu port 8090.");
        for (int i=0;i<10;i++) {
          if (responds()) {
            setStatus("PHP AKTIF ✓", "Dashboard dapat dibuka. " + shortText(output));
            return;
          }
          try {Thread.sleep(1800);}catch(InterruptedException e){break;}
        }
        setStatus("PHP belum aktif", "Perintah selesai, tetapi port 8090 belum merespons. " + shortText(output));
      } catch(Exception e) {
        setStatus("Belum berhasil", shortText(e.getMessage()));
      } finally {
        working(false);
      }
    }, "ssh-start").start();
  }
  private static String shortText(String s) {
    if (s==null||s.trim().length()==0) return "Tidak ada rincian dari server.";
    s=s.trim().replace("\r"," ").replace("\n","  ");
    if(s.length()>260) return s.substring(s.length()-260);
    return s;
  }
  private String execute(String pass) throws Exception {
    JSch jsch = new JSch();
    jsch.setHostKeyRepository(new PinnedKeyRepo());
    Session session = jsch.getSession("root",HOST,22);
    // Old WD OS3 SSH often provides only DSA host keys. Keep this exception local
    // to the NAS session; pin its SHA256 key before any password is sent.
    session.setConfig("server_host_key",session.getConfig("server_host_key")+",ssh-dss,ssh-rsa");
    session.setConfig("kex",session.getConfig("kex")+",diffie-hellman-group14-sha1,diffie-hellman-group1-sha1");
    session.setConfig("StrictHostKeyChecking","yes");
    session.setConfig("PreferredAuthentications","password");
    session.setPassword(pass);
    ChannelExec channel=null;
    try {
      session.connect(15000);
      setStatus("Menjalankan Boot EX4...", "Memakai startup yang sudah ada, tanpa ubah konfigurasi.");
      channel=(ChannelExec)session.openChannel("exec");
      channel.setCommand(CMD);
      channel.setInputStream(null);
      ByteArrayOutputStream error=new ByteArrayOutputStream();
      channel.setErrStream(error);
      InputStream stream=channel.getInputStream();
      channel.connect(12000);
      ByteArrayOutputStream output=new ByteArrayOutputStream();
      byte[] buffer=new byte[2048];
      long until=System.currentTimeMillis()+85000;
      while(System.currentTimeMillis()<until) {
        while(stream.available()>0) {
          int n=stream.read(buffer);
          if(n<0)break;
          if(output.size()<8192)output.write(buffer,0,n);
        }
        if(channel.isClosed())break;
        try {Thread.sleep(150);}catch(InterruptedException e){throw new Exception("Proses dibatalkan.");}
      }
      if(!channel.isClosed()) throw new Exception("Boot terlalu lama merespons. Server tidak diubah.");
      int exit=channel.getExitStatus();
      String out=output.toString("UTF-8")+" "+error.toString("UTF-8");
      if(exit!=0) throw new Exception("Boot EX4 mengembalikan kode "+exit+". "+out);
      return out;
    } finally {
      if(channel!=null) channel.disconnect();
      session.disconnect();
    }
  }
  private final class PinnedKeyRepo implements HostKeyRepository {
    private final SharedPreferences prefs=getSharedPreferences("ssh-host",MODE_PRIVATE);
    @Override public int check(String host,byte[] key) {
      try {
        String fingerprint="SHA256:"+Base64.encodeToString(
            MessageDigest.getInstance("SHA-256").digest(key),
            Base64.NO_WRAP | Base64.NO_PADDING);
        String known=prefs.getString(PREF,null);
        if(known!=null) return known.equals(fingerprint)?OK:CHANGED;
        CountDownLatch latch=new CountDownLatch(1);
        final boolean[] approved={false};
        ui.post(() -> {
          if(isFinishing()||isDestroyed()) {latch.countDown();return;}
          new AlertDialog.Builder(MainActivity.this)
            .setTitle("Konfirmasi identitas EX4")
            .setMessage("Server SSH pertama kali terhubung. Cocokkan sidik jari ini dengan ConnectBot:\n\n"+
                fingerprint+"\n\nTerima hanya bila sama. Password belum dikirim.")
            .setPositiveButton("Sama, percaya EX4",(d,w)->{approved[0]=true;latch.countDown();})
            .setNegativeButton("Batal",(d,w)->latch.countDown())
            .setOnCancelListener(d->latch.countDown())
            .show();
        });
        if(!latch.await(90,TimeUnit.SECONDS) || !approved[0]) return CHANGED;
        prefs.edit().putString(PREF,fingerprint).apply();
        return OK;
      }catch(Exception e){return CHANGED;}
    }
    @Override public void add(HostKey key,UserInfo ui){ /* handled by check() */ }
    @Override public void remove(String host,String type){ /* never silently forget pin */ }
    @Override public void remove(String host,String type,byte[] key){ /* never silently forget pin */ }
    @Override public String getKnownHostsRepositoryID(){return "EX4 pinned SHA256";}
    @Override public HostKey[] getHostKey(){return new HostKey[0];}
    @Override public HostKey[] getHostKey(String host,String type){return new HostKey[0];}
  }
}
