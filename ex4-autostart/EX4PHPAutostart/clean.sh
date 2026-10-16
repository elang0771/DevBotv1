#!/bin/sh
# Clean only the optional WD Apps info-page symlink.
PAGE=/var/www/apps/EX4PHPAutostart/index.html
case "$(readlink "$PAGE" 2>/dev/null)" in
  /mnt/HD/HD_a2/Nas_Prog/EX4PHPAutostart/web/index.html) rm -f "$PAGE" ;;
esac
rmdir /var/www/apps/EX4PHPAutostart 2>/dev/null || :
exit 0
