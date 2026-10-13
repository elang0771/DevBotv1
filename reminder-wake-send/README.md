# Reminder Wake + WhatsApp AutoSend test V0.8

Isolated experimental Android APK. V0.8 checks the visible scheduled caption and WhatsApp media editor before using an accessible Send control; it logs retry reasons without exposing recipient/caption, and does not claim message delivery. Installs separately from stable V0.4 and PASS/LOCK Wake-only V0.5.
Pipeline: alarm 60–600s → FullScreenIntent / WakeActivity → start ACTION_SEND with selected photo/caption/explicit jid → Accessibility observes WhatsApp media composer → matches scheduled caption → clicks media Send.
No phone PIN bypass. Does not read stored passwords. Only test with a phone number you control. Accessibility is manually enabled on each new app. The code disarms after one successful UI click and marks it clearly as click acceptance, not verified delivery.
If WhatsApp changes its UI so that media composer accessibility IDs are not available, it will NOT tap the generic Send button. Failure state can be inspected in diagnostic log for further fixes.
Original Wake-only V0.5 project and older Reminders remain unchanged. No keystore or secrets from the private V0.4 build are included.


V0.8 visual fallback: requires scheduled caption visible in Accessibility, WhatsApp active, screenshot showing near-black media editor header and geometrically round green Send button at lower-right. The screenshot is processed locally and not saved or uploaded. If screenshot capture is denied, no blind click is performed. Gesture success is not proof of delivery.
