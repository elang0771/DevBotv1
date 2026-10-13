# Reminder Wake + WhatsApp AutoSend test V1.0

Isolated experimental Android APK. V1.0 checks the visible scheduled caption and WhatsApp media editor before using an accessible Send control; it logs retry reasons without exposing recipient/caption, and does not claim message delivery. Installs separately from stable V0.4 and PASS/LOCK Wake-only V0.5.
Pipeline: alarm 60–600s → FullScreenIntent / WakeActivity → start ACTION_SEND with selected photo/caption/explicit jid → Accessibility observes WhatsApp media composer → matches scheduled caption → clicks media Send.
No phone PIN bypass. Does not read stored passwords. Only test with a phone number you control. Accessibility is manually enabled on each new app. The code disarms after one successful UI click and marks it clearly as click acceptance, not verified delivery.
If WhatsApp changes its UI so that media composer accessibility IDs are not available, it will NOT tap the generic Send button. Failure state can be inspected in diagnostic log for further fixes.
Original Wake-only V0.5 project and older Reminders remain unchanged. No keystore or secrets from the private V0.4 build are included.


V1.0 visual fallback: requires scheduled caption visible in Accessibility, WhatsApp active, screenshot showing near-black media editor header and geometrically round green Send button at lower-right. The screenshot is processed locally and not saved or uploaded. If screenshot capture is denied, no blind click is performed. Gesture success is not proof of delivery.


V1.0 fixes false-negative visual detection when the attached image fills the top of the screenshot; checks the bottom media-editor toolbar around the screen-relative Send button instead. Cancels the full-screen notification after wake and suppresses duplicate dispatches. Diagnostic includes a non-content reason on failed screenshot recognition. Does not prove WhatsApp delivery. 

V1.0 uses accessible caption text/contentDescription/hint where possible. For scheduled shares where WhatsApp renders caption but does not expose it through Accessibility, it permits a guarded, local-only, two-frame visual media editor check. Only the lower-most fully round green control on a dark bottom WhatsApp composer toolbar qualifies. Never click a green Send circle inside the photo itself. Use only for test sending to a number you control. An accepted UI click is NOT delivery proof. No screenshots are saved or transmitted.
