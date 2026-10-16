#!/bin/sh
# WD OS3 contract: install.sh <staging-app-directory> <Nas_Prog-parent>.
# Install only this app. Never change PHP, Apache, firmware hooks or shares.
set -eu
SRC=${1:-}
ROOT=${2:-}
NAME=EX4PHPAutostart
[ "$(id -u)" = 0 ] || exit 1
[ "$ROOT" = /mnt/HD/HD_a2/Nas_Prog ] || exit 1
[ -d "$ROOT" ] && [ ! -L "$ROOT" ] || exit 1
[ -n "$SRC" ] && [ -d "$SRC" ] && [ ! -L "$SRC" ] || exit 1
[ -f "$SRC/apkg.rc" ] && grep -q '^Package: EX4PHPAutostart$' "$SRC/apkg.rc" || exit 1
[ ! -e "$ROOT/$NAME" ] && [ ! -L "$ROOT/$NAME" ] || exit 1
cp -R "$SRC" "$ROOT/$NAME" || exit 1
chown -R 0:0 "$ROOT/$NAME" || exit 1
chmod 755 "$ROOT/$NAME" "$ROOT/$NAME/web" || exit 1
chmod 700 "$ROOT/$NAME"/*.sh || exit 1
chmod 644 "$ROOT/$NAME/apkg.rc" "$ROOT/$NAME/icon.png" "$ROOT/$NAME/web/index.html" || exit 1
exit 0
