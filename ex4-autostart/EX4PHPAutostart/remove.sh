#!/bin/sh
# Only remove this single WD OS3 app, never shared PHP/Boot/WebApps.
set -eu
DEST=${1:-}
EXPECTED=/mnt/HD/HD_a2/Nas_Prog/EX4PHPAutostart
[ "$DEST" = "$EXPECTED" ] || exit 1
[ "$(id -u)" = 0 ] || exit 1
[ -d "$DEST" ] && [ ! -L "$DEST" ] || exit 1
[ -f "$DEST/.install-ok" ] || exit 1
grep -qx 'installed=1.0.3' "$DEST/.install-ok" || exit 1
grep -qx 'Package: EX4PHPAutostart' "$DEST/apkg.rc" || exit 1
rm -rf "$DEST"
