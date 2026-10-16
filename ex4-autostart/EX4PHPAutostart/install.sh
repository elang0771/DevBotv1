#!/bin/sh
# WD OS3 supplies: install.sh <staging-app-directory> <Nas_Prog>.
# WD may create the app's destination folder BEFORE invoking install.sh.
# Only install our own package; never alter the existing PHP, Dashboard, or NAS.
set -eu
SRC=${1:-}
ROOT=${2:-}
NAME=EX4PHPAutostart
DEST="$ROOT/$NAME"
[ "$(id -u)" = 0 ] || exit 1
[ "$ROOT" = /mnt/HD/HD_a2/Nas_Prog ] || exit 1
[ -d "$ROOT" ] && [ ! -L "$ROOT" ] || exit 1
[ -n "$SRC" ] && [ -d "$SRC" ] && [ ! -L "$SRC" ] || exit 1
[ -f "$SRC/apkg.rc" ] && grep -q '^Package: EX4PHPAutostart$' "$SRC/apkg.rc" || exit 1
[ -f "$SRC/start.sh" ] && [ -f "$SRC/init.sh" ] || exit 1
if [ -e "$DEST" ] || [ -L "$DEST" ]; then
  [ -d "$DEST" ] && [ ! -L "$DEST" ] || exit 1
  # Refuse to overwrite unknown data; a new WD-created empty folder is fine.
  [ -z "$(ls -A "$DEST")" ] || exit 1
else
  mkdir "$DEST" || exit 1
fi
chown 0:0 "$DEST" || exit 1
chmod 755 "$DEST" || exit 1
# Copy contents, not the parent folder, avoiding nested EX4PHPAutostart/.
cp -R "$SRC"/. "$DEST"/ || exit 1
[ -f "$DEST/start.sh" ] && [ -f "$DEST/apkg.rc" ] || exit 1
chown -R 0:0 "$DEST" || exit 1
chmod 755 "$DEST" "$DEST/web" || exit 1
chmod 700 "$DEST"/*.sh || exit 1
chmod 644 "$DEST/apkg.rc" "$DEST/icon.png" "$DEST/web/index.html" || exit 1
exit 0
