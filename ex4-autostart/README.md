# WD My Cloud EX4 PHP Autostart OS3
## Installer contract correction — v1.0.4 candidate
WDCommunity reference OS3 package scripts `wdpk/entware/install.sh`,
`wdpk/nzbget/install.sh`, and `wdpk/myshellscript/common.sh`
use `$1` as the extracted app folder, and `$2` as the **Nas_Prog parent directory**.
The prior v1.0.2 and v1.0.3 installer incorrectly expected the second argument
to be the final application directory. The NAS observation after install was
`Nas_Prog/EX4PHPAutostart/apkg.xml` only.

This new candidate adds a GitHub Actions simulation for the correct contract and
the firmware-created `apkg.xml` folder. It does not touch the working PHP 8090,
Dashboard v2.3.1, APC, or NAS configs.

**Never claim autostart PASS until manually checked on the NAS after reboot.**
