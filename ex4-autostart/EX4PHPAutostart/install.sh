#!/bin/sh
# WD My Cloud OS3 SDK:
#   /mnt/HD/HD_a2/Nas_Prog/_install/install.sh \
#     /mnt/HD/HD_a2/Nas_Prog/_install \
#     /mnt/HD/HD_a2/Nas_Prog/EX4PHPAutostart
# $2 is the FULL APP DESTINATION (not Nas_Prog). No changes to other apps/NAS.
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
[ -f "$SRC/apkg.rc" ] && grep -q '^Package: EX4PHPAutostart$' "$SRC/apkg.rc" || exit 1
[ -f "$SRC/start.sh" ] && [ -f "$SRC/init.sh" ] && [ -f "$SRC/remove.sh" ] || exit 1
if [ -e "$DEST" ] || [ -L "$DEST" ]; then
  [ -d "$DEST" ] && [ ! -L "$DEST" ] || exit 1
  # A folder precreated by WD is allowed only if empty.
  [ -z "$(ls -A "$DEST")" ] || exit 1
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
printf 'installed=1.0.2\n' > "$DEST/.install-ok" || exit 1
chmod 600 "$DEST/.install-ok" || exit 1
exit 0
