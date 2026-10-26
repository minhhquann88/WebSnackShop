#!/usr/bin/env bash
# One-time TLS bootstrap: nginx cannot start without certificates, and certbot needs nginx to answer the
# ACME challenge. So: create a throw-away self-signed cert, start nginx, obtain real certs, reload.
#   ./scripts/init-letsencrypt.sh            uses API_DOMAIN (+ WEB_DOMAIN if set) and ACME_EMAIL from .env
#   STAGING=1 ./scripts/init-letsencrypt.sh  test against Let's Encrypt staging first (no rate limits)
#   DUMMY_ONLY=1 ./scripts/init-letsencrypt.sh  stop after the self-signed step (local testing)
# DNS A records for every domain must already point at this server. Run from infra/.
set -euo pipefail
cd "$(dirname "$0")/.."
source scripts/lib.sh
require_env_file

API_DOMAIN=$(env_get API_DOMAIN)
WEB_DOMAIN=$(env_get WEB_DOMAIN)
ACME_EMAIL=$(env_get ACME_EMAIL)
[ -n "$API_DOMAIN" ] || { echo "API_DOMAIN is not set in .env"; exit 1; }

DOMAINS=("$API_DOMAIN")
[ -n "$WEB_DOMAIN" ] && DOMAINS+=("$WEB_DOMAIN")
STAGING_ARGS=()
[ "${STAGING:-0}" = "1" ] && STAGING_ARGS=(--staging)

for domain in "${DOMAINS[@]}"; do
  log "dummy certificate for $domain"
  "${COMPOSE[@]}" run --rm --no-deps --entrypoint sh certbot -c "\
    mkdir -p /etc/letsencrypt/live/$domain && \
    openssl req -x509 -nodes -newkey rsa:2048 -days 1 \
      -keyout /etc/letsencrypt/live/$domain/privkey.pem \
      -out /etc/letsencrypt/live/$domain/fullchain.pem -subj /CN=localhost >/dev/null 2>&1"
done

log "starting nginx"
"${COMPOSE[@]}" up -d --no-deps nginx

if [ "${DUMMY_ONLY:-0}" = "1" ]; then
  log "DUMMY_ONLY=1: leaving self-signed certificates in place"
  exit 0
fi

[ -n "$ACME_EMAIL" ] || { echo "ACME_EMAIL is not set in .env"; exit 1; }
for domain in "${DOMAINS[@]}"; do
  log "requesting real certificate for $domain"
  "${COMPOSE[@]}" run --rm --no-deps --entrypoint sh certbot -c "\
    rm -rf /etc/letsencrypt/live/$domain /etc/letsencrypt/archive/$domain /etc/letsencrypt/renewal/$domain.conf"
  "${COMPOSE[@]}" run --rm --no-deps --entrypoint certbot certbot certonly --webroot -w /var/www/certbot \
    "${STAGING_ARGS[@]}" --email "$ACME_EMAIL" --agree-tos --no-eff-email -d "$domain"
done

log "reloading nginx"
"${COMPOSE[@]}" exec -T nginx nginx -s reload
log "done. Certificates renew automatically (certbot service); deploy.sh reloads nginx."
