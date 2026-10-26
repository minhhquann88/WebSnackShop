#!/bin/sh
# Runs inside the `backup` container (postgres:17-alpine): custom-format pg_dump + uploads tarball,
# kept for BACKUP_RETENTION_DAYS. Copy /backups off the VPS (see docs/08 §9: rclone on the host).
#   backup.sh [label]     label defaults to "manual" (scheduled / pre-deploy are used by cron and deploy.sh)
set -eu

LABEL=${1:-manual}
STAMP=$(date +%Y%m%d_%H%M%S)
DIR=/backups
RETENTION=${BACKUP_RETENTION_DAYS:-14}
mkdir -p "$DIR"

DB_FILE="$DIR/snackshop_${STAMP}_${LABEL}.dump"
pg_dump --format=custom --compress=9 --no-owner --file="$DB_FILE.partial"
mv "$DB_FILE.partial" "$DB_FILE"

if [ -d /uploads ]; then
  tar -czf "$DIR/uploads_${STAMP}_${LABEL}.tar.gz" -C /uploads .
fi

# Retention (pre-deploy backups count too).
find "$DIR" -name 'snackshop_*.dump' -mtime +"$RETENTION" -delete
find "$DIR" -name 'uploads_*.tar.gz' -mtime +"$RETENTION" -delete

echo "backup ok: $DB_FILE ($(du -h "$DB_FILE" | cut -f1))"
