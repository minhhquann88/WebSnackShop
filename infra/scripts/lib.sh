#!/usr/bin/env bash
# Shared helpers for the infra scripts. Sourced, not executed.

# Compose command for the production stack (run from infra/).
COMPOSE=(docker compose -f docker-compose.prod.yml)

# Reads KEY from .env without `source` (values may contain spaces, '<' or other shell syntax).
env_get() {
  local key=$1 default=${2:-}
  local line
  line=$(grep -E "^${key}=" .env 2>/dev/null | tail -n 1 || true)
  if [ -n "$line" ]; then
    printf '%s' "${line#*=}"
  else
    printf '%s' "$default"
  fi
}

log() { printf '[%s %s] %s\n' "$(basename "$0" .sh)" "$(date '+%F %T')" "$*"; }

require_env_file() {
  [ -f .env ] || { echo "infra/.env is missing (copy .env.example and fill it in)"; exit 1; }
}
