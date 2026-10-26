#!/usr/bin/env bash
# One-time hardening of a fresh Ubuntu 24.04 VPS (docs/08 §8). Run as root:
#   curl -fsSL <raw url of this file> | bash -s -- "<your ssh public key>"
# Creates user `deploy` (SSH key only, member of docker), installs Docker, ufw (22/80/443), fail2ban,
# unattended upgrades and a 2 GB swap file. Review before running; it disables SSH password login.
set -euo pipefail

PUBKEY=${1:?usage: server-setup.sh "<ssh-public-key>"}
APP_DIR=/opt/snackshop

apt-get update
DEBIAN_FRONTEND=noninteractive apt-get upgrade -y
DEBIAN_FRONTEND=noninteractive apt-get install -y ca-certificates curl ufw fail2ban unattended-upgrades git

# Docker Engine + compose plugin (official convenience script).
if ! command -v docker >/dev/null; then
  curl -fsSL https://get.docker.com | sh
fi

# Deploy user.
id deploy >/dev/null 2>&1 || adduser --disabled-password --gecos "" deploy
usermod -aG docker deploy
install -d -m 700 -o deploy -g deploy /home/deploy/.ssh
grep -qF "$PUBKEY" /home/deploy/.ssh/authorized_keys 2>/dev/null || echo "$PUBKEY" >> /home/deploy/.ssh/authorized_keys
chown deploy:deploy /home/deploy/.ssh/authorized_keys && chmod 600 /home/deploy/.ssh/authorized_keys
install -d -o deploy -g deploy "$APP_DIR"

# SSH: keys only, no root login.
sed -i 's/^#\?PasswordAuthentication .*/PasswordAuthentication no/; s/^#\?PermitRootLogin .*/PermitRootLogin no/' /etc/ssh/sshd_config
systemctl reload ssh || systemctl reload sshd

# Firewall.
ufw default deny incoming
ufw default allow outgoing
ufw allow 22/tcp
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable

systemctl enable --now fail2ban
dpkg-reconfigure -f noninteractive unattended-upgrades

# Swap (helps the 4 GB plan during image pulls and .NET start-up).
if [ ! -f /swapfile ]; then
  fallocate -l 2G /swapfile && chmod 600 /swapfile && mkswap /swapfile && swapon /swapfile
  echo '/swapfile none swap sw 0 0' >> /etc/fstab
fi

timedatectl set-timezone Asia/Ho_Chi_Minh
echo "done. Next: as 'deploy', copy infra/ to $APP_DIR/infra, create .env, run scripts/init-letsencrypt.sh then scripts/deploy.sh <tag>."
