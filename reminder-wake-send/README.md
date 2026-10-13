# Reminder Wake + WhatsApp AutoSend test V0.6

Isolated experimental Android APK. Installs separately from stable V0.4 and PASS/LOCK Wake-only V0.5.
Pipeline: alarm 60–600s → FullScreenIntent / WakeActivity → start ACTION_SEND with selected photo/caption/explicit jid → Accessibility observes WhatsApp media composer → matches scheduled caption → clicks media Send.
No phone PIN bypass. Does not read stored passwords. Only test with a phone number you control. Accessibility is manually enabled on each new app. The code disarms after one successful UI click and marks it clearly as click acceptance, not verified delivery.
If WhatsApp changes its UI so that media composer accessibility IDs are not available, it will NOT tap the generic Send button. Failure state can be inspected in diagnostic log for further fixes.
Original Wake-only V0.5 project and older Reminders remain unchanged. No keystore or secrets from the private V0.4 build are included.
