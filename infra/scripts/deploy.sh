#!/usr/bin/env bash
# Deploys an API image tag on the VPS: pull → pre-deploy backup → apply migrations shipped in the image →
# restart api → wait for readiness → roll back to the previous tag on failure.
#
#   ./scripts/deploy.sh <tag>        e.g. ./scripts/deploy.sh v1.0.0   (or a git sha)
#   ./scripts/deploy.sh rollback     re-deploy the last known-good tag (schema is NOT rolled back)
#   SKIP_PULL=1 ./scripts/deploy.sh local   use an image that only exists locally (testing)
#
# Run from infra/. Requires infra/.env. Migrations must be backward compatible (docs/06 §8),
# because a rollback only swaps the application image.
set -euo pipefail
cd "$(dirname "$0")/.."
source scripts/lib.sh
require_env_file

STATE_FILE=.last_good_tag
TARGET=${1:?usage: deploy.sh <tag>|rollback}
PG_USER=$(env_get POSTGRES_USER snackshop)
PG_DB=$(env_get POSTGRES_DB snackshop)

if [ "$TARGET" = "rollback" ]; then
  TARGET=$(cat "$STATE_FILE" 2>/dev/null || true)
  [ -n "$TARGET" ] || { log "no known-good tag recorded"; exit 1; }
  log "rolling back to $TARGET"
fi

PREVIOUS=$(cat "$STATE_FILE" 2>/dev/null || echo "")

wait_ready() {
  for _ in $(seq 1 40); do
    if "${COMPOSE[@]}" exec -T api wget -qO- http://localhost:8080/health/ready >/dev/null 2>&1; then
      return 0
    fi
    sleep 3
  done
  return 1
}

if [ "${SKIP_PULL:-0}" != "1" ]; then
  log "pulling api:$TARGET"
  API_TAG=$TARGET "${COMPOSE[@]}" pull api
fi

log "ensuring postgres is up"
"${COMPOSE[@]}" up -d --wait postgres

log "pre-deploy backup"
"${COMPOSE[@]}" run --rm --no-deps --entrypoint /bin/sh backup /usr/local/bin/backup.sh pre-deploy

log "applying migrations shipped in the image"
API_TAG=$TARGET "${COMPOSE[@]}" run --rm --no-deps --entrypoint cat api /app/migrations.sql \
  | "${COMPOSE[@]}" exec -T postgres psql -v ON_ERROR_STOP=1 -q -U "$PG_USER" -d "$PG_DB"

log "starting api $TARGET"
API_TAG=$TARGET "${COMPOSE[@]}" up -d --no-deps api

if wait_ready; then
  echo "$TARGET" > "$STATE_FILE"
  "${COMPOSE[@]}" up -d backup certbot nginx
  # Pick up renewed certificates and any template changes.
  "${COMPOSE[@]}" exec -T nginx nginx -s reload >/dev/null 2>&1 || true
  docker image prune -f >/dev/null 2>&1 || true
  log "deployed $TARGET"
  exit 0
fi

log "api did not become ready; recent logs:"
"${COMPOSE[@]}" logs --tail 80 api || true

if [ -n "$PREVIOUS" ] && [ "$PREVIOUS" != "$TARGET" ]; then
  log "rolling back to $PREVIOUS"
  API_TAG=$PREVIOUS "${COMPOSE[@]}" up -d --no-deps api
  if wait_ready; then log "rollback to $PREVIOUS succeeded"; else log "ROLLBACK FAILED - investigate now"; fi
fi
exit 1
