#!/bin/sh
# WD OS3 SDK: install.sh <Nas_Prog/_install> <Nas_Prog/EX4PHPAutostart>
# EX4 firmware may precreate app destination containing ONLY apkg.xml.
# No writes outside this app directory. PHP 8090 and WD Dashboard stay untouched.
set -eu
SRC=${1:-}
DEST=${2:-}
NAS=/mnt/HD/HD_a2/Nas_Prog
EXPECTED="$NAS/EX4PHPAutostart"
[ "$(id -u)" = 0 ] || exit 1
[ "$SRC" = "$NAS/_install" ] || exit 1
[ "$DEST" = "$EXPECTED" ] || exit 1
[ -d "$NAS" ] && [ ! -L "$NAS" ] || exit 1
[ -d "$SRC" ] && [ ! -L "$SRC" ] || exit 1
[ -f "$SRC/apkg.rc" ] && [ ! -L "$SRC/apkg.rc" ] || exit 1
grep -q '^Package: EX4PHPAutostart$' "$SRC/apkg.rc" || exit 1
[ -f "$SRC/start.sh" ] && [ -f "$SRC/init.sh" ] && [ -f "$SRC/remove.sh" ] || exit 1
if [ -e "$DEST" ] || [ -L "$DEST" ]; then
  [ -d "$DEST" ] && [ ! -L "$DEST" ] || exit 1
  # Allow empty folder or the firmware-created apkg.xml ONLY.
  case "$(ls -A "$DEST")" in
    '') ;;
    apkg.xml)
      [ -f "$DEST/apkg.xml" ] && [ ! -L "$DEST/apkg.xml" ] || exit 1
      grep -q '<name>EX4PHPAutostart</name>' "$DEST/apkg.xml" || exit 1
      ;;
    *) exit 1 ;;
  esac
else
  mkdir "$DEST" || exit 1
fi
chown 0:0 "$DEST" || exit 1
chmod 755 "$DEST" || exit 1
cp -R "$SRC"/. "$DEST"/ || exit 1
[ -f "$DEST/start.sh" ] && [ -f "$DEST/apkg.rc" ] && [ -d "$DEST/web" ] || exit 1
chown -R 0:0 "$DEST" || exit 1
chmod 755 "$DEST" "$DEST/web" || exit 1
chmod 700 "$DEST"/*.sh || exit 1
chmod 644 "$DEST/apkg.rc" "$DEST/icon.png" "$DEST/web/index.html" || exit 1
printf 'installed=1.0.3\n' > "$DEST/.install-ok" || exit 1
chmod 600 "$DEST/.install-ok" || exit 1
echo 'EX4 Autostart installation PASS: start.sh installed' >&2
exit 0
