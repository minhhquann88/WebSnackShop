#!/usr/bin/env bash
# Restores a pg_dump file into a scratch database, verifies it, then (after confirmation) swaps it in.
#   ./scripts/restore.sh backups/snackshop_20261022_030000_scheduled.dump [--yes]
# Practise this before go-live and every quarter (docs/08 §9). Run from infra/.
set -euo pipefail
cd "$(dirname "$0")/.."
source scripts/lib.sh
require_env_file

FILE=${1:?usage: restore.sh <dump-file> [--yes]}
CONFIRM=${2:-}
[ -f "$FILE" ] || { echo "no such file: $FILE"; exit 1; }

PG_USER=$(env_get POSTGRES_USER snackshop)
PG_DB=$(env_get POSTGRES_DB snackshop)
SCRATCH="${PG_DB}_restore"

psql_admin() { "${COMPOSE[@]}" exec -T postgres psql -v ON_ERROR_STOP=1 -U "$PG_USER" -d postgres "$@"; }

log "restoring $FILE into $SCRATCH"
psql_admin -c "DROP DATABASE IF EXISTS \"$SCRATCH\";" -c "CREATE DATABASE \"$SCRATCH\";"
"${COMPOSE[@]}" exec -T postgres pg_restore --no-owner -U "$PG_USER" -d "$SCRATCH" < "$FILE"

log "row counts in restored copy:"
"${COMPOSE[@]}" exec -T postgres psql -U "$PG_USER" -d "$SCRATCH" -At -c \
  "SELECT 'orders=' || count(*) FROM orders UNION ALL SELECT 'products=' || count(*) FROM products UNION ALL SELECT 'users=' || count(*) FROM asp_net_users;"

if [ "$CONFIRM" != "--yes" ]; then
  read -r -p "Replace live database '$PG_DB' with this copy? The API will be stopped. [y/N] " answer
  [ "$answer" = "y" ] || { log "kept $SCRATCH for inspection; live database untouched"; exit 0; }
fi

"${COMPOSE[@]}" stop api
psql_admin -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$PG_DB';" \
           -c "ALTER DATABASE \"$PG_DB\" RENAME TO \"${PG_DB}_before_restore_$(date +%Y%m%d%H%M%S)\";" \
           -c "ALTER DATABASE \"$SCRATCH\" RENAME TO \"$PG_DB\";"
"${COMPOSE[@]}" start api
log "restore complete; the previous database was kept under a *_before_restore_* name"
