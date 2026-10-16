#!/bin/sh
# WD Apps startup: use only already verified root-private Boot v4.1.
# NEVER invoke a shell script from an SMB share as root.
set -u
BOOT=/usr/local/config/ex4-webapps-internal/start.sh
BASE=/usr/local/config/ex4-webapps-internal
STATE=/var/run/ex4php-autostart
EXPECTED=303378853743b92cef0e3ae561bd430e783cdddb646cdab4b1e39f18451d5a2a
[ "$(id -u)" = 0 ] || exit 1
[ -x /usr/bin/php ] && [ -f "$BOOT" ] && [ -d "$BASE" ] && [ ! -L "$BOOT" ] || exit 1
/usr/bin/php -r '
foreach(array($argv[1],$argv[2]) as $p){
  $s=@lstat($p);
  if(!$s || is_link($p) || (int)$s["uid"]!==0 || ($s["mode"] & 0022)!==0)exit(1);
}
if(@hash_file("sha256",$argv[2])!==$argv[3])exit(1);
' "$BASE" "$BOOT" "$EXPECTED" || exit 1
[ -d "$STATE" ] || mkdir "$STATE" || exit 1
# Do not block WD Apps startup. Boot script checks for an existing live PHP service.
(
  for pause in 0 12 12 12 12 12 12 12 12 12; do
    [ "$pause" = 0 ] || sleep "$pause"
    /bin/sh "$BOOT" && exit 0
  done
  exit 1
) >"$STATE/start.log" 2>&1 </dev/null &
exit 0
