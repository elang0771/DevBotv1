# EX4 PHP Starter — Android (Tablet)
A minimal Android single-screen SSH launcher for the existing WD My Cloud EX4 PHP built-in web server.

- Connects to `root@192.168.18.22:22`, executes **only**
  `sh /usr/local/config/ex4-webapps-internal/start.sh`.
- Opens `http://192.168.18.22:8090/dashboard.php` in the browser.
- Checks existing service before starting; avoids launching duplicate processes.
- Requires SSH password for startup, **never persists it**.
- On first connection, presents SSH host public-key SHA256 fingerprint for user confirmation and pins it locally. Changed keys are rejected.
- WD OS3 DSA compatibility enabled for this **one host** (known legacy risk).
- Does NOT alter WD firmware, APC, Dashboard 2.3.1, server configuration, user files, or install new PHP.
- Suitable only on user's trusted Wi-Fi/LAN; no public access.
- The app itself does not guarantee that the NAS root-private boot script will succeed. Success is reported only when the dashboard responds after boot.

Build: GitHub Actions `EX4 Tablet Starter APK`. Uses a debug signature; upgrades built in other workflow runs may require uninstall/reinstall. No user data is stored beyond the pinned host fingerprint.
