#!/bin/sh
# Register a tiny read-only WD Apps info page. Does not touch PHP port 8090.
set -eu
[ "$(id -u)" = 0 ] || exit 1
APP=/mnt/HD/HD_a2/Nas_Prog/EX4PHPAutostart
DEST=/var/www/apps/EX4PHPAutostart
[ -f "$APP/web/index.html" ] || exit 1
[ -d /var/www/apps ] || exit 0
[ ! -L "$DEST" ] || exit 1
[ -d "$DEST" ] || mkdir "$DEST" || exit 1
[ ! -e "$DEST/index.html" ] && [ ! -L "$DEST/index.html" ] || exit 0
ln -s "$APP/web/index.html" "$DEST/index.html" || exit 1
exit 0
