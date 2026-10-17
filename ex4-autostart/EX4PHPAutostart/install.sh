#!/bin/sh
# WD OS3 reference contracts (WDCommunity):
#   install.sh "$1" "$2" where $1 is extracted app directory
#   and $2 is the PARENT /mnt/HD/HD_a2/Nas_Prog.
# No changes to existing PHP 8090, Dashboard, or firmware boot hooks.
set -eu
SRC=${1:-}
NAS=${2:-}
APP=EX4PHPAutostart
EXPECTED=/mnt/HD/HD_a2/Nas_Prog
[ "$(id -u)" = 0 ] || exit 1
[ "$NAS" = "$EXPECTED" ] || exit 1
[ -d "$NAS" ] && [ ! -L "$NAS" ] || exit 1
[ -n "$SRC" ] && [ -d "$SRC" ] && [ ! -L "$SRC" ] || exit 1
[ "${SRC##*/}" = "$APP" ] || exit 1
# Must be a WD stage, never trust a script read from WebApps/SMB.
case "$SRC" in
  "$NAS/_install/$APP"|/tmp/*/"$APP"|/var/tmp/*/"$APP") ;;
  *) exit 1 ;;
esac
[ -f "$SRC/apkg.rc" ] && [ ! -L "$SRC/apkg.rc" ] || exit 1
grep -qx 'Package: EX4PHPAutostart' "$SRC/apkg.rc" || exit 1
for f in start.sh init.sh remove.sh apkg.rc icon.png; do
  [ -f "$SRC/$f" ] && [ ! -L "$SRC/$f" ] || exit 1
done
DEST="$NAS/$APP"
if [ -e "$DEST" ] || [ -L "$DEST" ]; then
  [ -d "$DEST" ] && [ ! -L "$DEST" ] || exit 1
  case "$(ls -A "$DEST")" in
    '') ;;
    apkg.xml)
      [ -f "$DEST/apkg.xml" ] && [ ! -L "$DEST/apkg.xml" ] || exit 1
      grep -q '<name>EX4PHPAutostart</name>' "$DEST/apkg.xml" || exit 1 ;;
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
printf 'installed=1.0.4\n' > "$DEST/.install-ok" || exit 1
chmod 600 "$DEST/.install-ok" || exit 1
echo 'EX4PHPAutostart INSTALL PASS - start.sh present' >&2
exit 0
