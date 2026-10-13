# Reminder Wake Only V0.5
Standalone Android wake-screen diagnostic test. Does not send WhatsApp and does not change Reminder V0.4.
On Android 16 / Samsung S23 Ultra, enable Notification, Exact alarm and Full-screen intent permissions in the app.
Schedule 60 seconds, press Home and power off screen. PASS only if the green WakeActivity appears automatically without touching device.
Logs distinguish ALARM_FIRED from ACTIVITY_RESUMED and WINDOW_FOCUSED.
