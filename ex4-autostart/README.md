# EX4 PHP Autostart OS3 v1.0.0 — Build candidate
This isolated branch only creates a WDMyCloudEX4 OS3 app package. It does not install anything on the NAS, change Dashboard 2.3.1, touch PHP 8090 or NAS config.

The package calls the root-private known SHA256 of **Boot v4.1**. It never launches scripts from SMB shares. Stop/disable does **not** stop the existing PHP server. WD Apps manager controls future boot behavior; reboot verification is still required.

Source is published in a dedicated build branch and not merged to DevBotv1 main. It contains no passwords or tokens.
