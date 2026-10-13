package com.elang.reminderwakesend;

import android.accessibilityservice.AccessibilityService;
import android.content.SharedPreferences;
import android.os.Handler;
import android.os.Looper;
import android.view.accessibility.AccessibilityEvent;
import android.view.accessibility.AccessibilityNodeInfo;
import java.util.List;
import java.util.Locale;

public class AutoSendAccessibilityService extends AccessibilityService {
    private final Handler handler=new Handler(Looper.getMainLooper());
    private boolean waiting=false,waLogged=false,captionLogged=false;
    private long lastJob=0;

    @Override public void onAccessibilityEvent(AccessibilityEvent e) {
        if(e==null||e.getPackageName()==null||
           !MainActivity.WA.contentEquals(e.getPackageName()))return;
        SharedPreferences p=Diag.prefs(this);
        long until=p.getLong("armed_until",0L), now=System.currentTimeMillis();
        if(until<=now)return;
        if(!waLogged || lastJob!=until){
            waLogged=true;captionLogged=false;lastJob=until;
            Diag.log(this,"WHATSAPP_ACCESSIBILITY_OBSERVED");
        }
        if(!waiting) {
            waiting=true;
            handler.postDelayed(this::inspectAndSend,1500);
        }
    }
    private void inspectAndSend() {
        SharedPreferences p=Diag.prefs(this);
        long until=p.getLong("armed_until",0L);
        if(until<=System.currentTimeMillis()){waiting=false;return;}
        if(p.getBoolean("click_in_progress",false)){waiting=false;return;}
        AccessibilityNodeInfo root=getRootInActiveWindow();
        if(root==null || root.getPackageName()==null ||
              !MainActivity.WA.contentEquals(root.getPackageName())){
            handler.postDelayed(this::inspectAndSend,1400);
            return;
        }
        String expected=p.getString("expected_caption","");
        String signature=expected.length()>24?expected.substring(0,24):expected;
        boolean match=!signature.isEmpty() && hasCaption(root,signature,0);
        if(!match){
            // Never send if the scheduled caption is not visible; prevents mis-taps.
            handler.postDelayed(this::inspectAndSend,1400);
            return;
        }
        if(!captionLogged){Diag.log(this,"CAPTION_VISIBLE_MATCHED");captionLogged=true;}
        AccessibilityNodeInfo button=findMediaSend(root,0);
        if(button==null){
            handler.postDelayed(this::inspectAndSend,1400);
            return;
        }
        // Require media-specific send UI, not the normal chat textbox send button.
        p.edit().putBoolean("click_in_progress",true).commit();
        boolean action=click(button);
        if(action){
            p.edit().putLong("armed_until",0L).putString("state","SEND_CLICK_ACCEPTED_NOT_DELIVERY_PROOF").commit();
            Diag.log(this,"SEND_BUTTON_CLICK_ACCEPTED");
        }else{
            p.edit().putBoolean("click_in_progress",false).commit();
            Diag.log(this,"SEND_BUTTON_CLICK_FAILED_RETRY");
            handler.postDelayed(this::inspectAndSend,1400);return;
        }
        waiting=false;
    }
    private boolean hasCaption(AccessibilityNodeInfo node,String expected,int depth){
        if(node==null||depth>35)return false;
        String text=safe(node.getText());
        if(text.contains(expected))return true;
        for(int i=0;i<node.getChildCount();i++){
            if(hasCaption(node.getChild(i),expected,depth+1))return true;
        }
        return false;
    }
    private AccessibilityNodeInfo findMediaSend(AccessibilityNodeInfo node,int depth){
        if(node==null || depth>35)return null;
        String id=safe(node.getViewIdResourceName()).toLowerCase(Locale.ROOT);
        String desc=safe(node.getContentDescription()).toLowerCase(Locale.ROOT);
        // Accept WA media composer only; no generic Send nodes in chat.
        boolean media = id.contains("send_media")||id.contains("media_send")||
          id.endsWith(":id/send_media_button");
        if(media && node.isVisibleToUser())return node;
        // WA releases sometimes name media preview send the generic 'send' id.
        // Do not use generic fallback blindly: requiring a media preview marker.
        if((id.endsWith(":id/send")||id.endsWith(":id/send_button")) &&
                node.isVisibleToUser() && hasMediaPreview(node.getParent(),0))return node;
        for(int i=0;i<node.getChildCount();i++){
            AccessibilityNodeInfo found=findMediaSend(node.getChild(i),depth+1);
            if(found!=null)return found;
        }
        return null;
    }
    private boolean hasMediaPreview(AccessibilityNodeInfo n,int d){
        if(n==null||d>4)return false;
        String id=safe(n.getViewIdResourceName()).toLowerCase(Locale.ROOT);
        if(id.contains("media_preview")||id.contains("photo_preview")||id.contains("media_caption"))return true;
        return hasMediaPreview(n.getParent(),d+1);
    }
    private boolean click(AccessibilityNodeInfo n){
        AccessibilityNodeInfo current=n;
        for(int d=0;d<6&&current!=null;d++){
            if(current.isClickable())return current.performAction(AccessibilityNodeInfo.ACTION_CLICK);
            current=current.getParent();
        }
        return n.performAction(AccessibilityNodeInfo.ACTION_CLICK);
    }
    private String safe(CharSequence s){return s==null?"":s.toString();}
    @Override public void onInterrupt(){waiting=false;}
    @Override public void onDestroy(){handler.removeCallbacksAndMessages(null);super.onDestroy();}
}
