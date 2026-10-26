# 08 — Hạ tầng, cấu hình, CI/CD, vận hành

Quyết định: Docker Compose cho máy dev, **VPS Linux + Docker Compose + Nginx** cho production, GitHub Actions cho CI/CD. Lý do: [ADR-0004](adr/0004-monorepo-docker-vps.md).

## 1. Môi trường

| Môi trường | Ở đâu | DB | Mục đích |
|---|---|---|---|
| `Development` | Máy dev, `dotnet run` + Docker Postgres | `snackshop_dev` | Code hằng ngày, seed data mẫu, Scalar UI bật |
| `Test` | CI + local `dotnet test` | Testcontainers (tạm) | Integration test |
| `Staging` | Cùng VPS, compose project khác, subdomain `staging.` | `snackshop_staging` | Thử trước khi lên prod, auto deploy từ `develop` |
| `Production` | VPS, subdomain `api.` | `snackshop` | Deploy từ `main` khi tag |

Giai đoạn đầu Staging có thể bỏ nếu VPS yếu; bật lại khi có khách thật.

## 2. Thư mục `infra/`

```
infra/
  docker-compose.yml           ← dev: postgres, mailpit
  docker-compose.prod.yml      ← prod: postgres, api, nginx, certbot, backup (+ web, profile tùy chọn)
  .env.example                 ← mọi biến production, giá trị giả (copy thành infra/.env trên VPS)
  nginx/
    templates/api.conf.template   ← render bằng envsubst của image nginx (API_DOMAIN, WEB_DOMAIN)
    snippets/                     ← tls.conf, security-headers.conf, proxy-api.conf
    optional/web.conf.template    ← chỉ khi tự host frontend
  scripts/
    lib.sh                     ← helper chung (đọc .env an toàn, không `source`)
    server-setup.sh            ← hardening Ubuntu 24.04 lần đầu (chạy bằng root)
    init-letsencrypt.sh        ← cấp chứng chỉ TLS lần đầu
    deploy.sh                  ← pull → backup → migration → up → health → rollback
    backup.sh                  ← chạy trong container backup (cron 03:00) và trước mỗi deploy
    restore.sh                 ← khôi phục vào DB tạm, kiểm tra, rồi hoán đổi
  backups/                     ← sinh ra trên VPS (gitignored)
```

### `docker-compose.yml` (dev)

```yaml
services:
  postgres:
    image: postgres:17-alpine
    environment:
      POSTGRES_USER: snackshop
      POSTGRES_PASSWORD: snackshop
      POSTGRES_DB: snackshop_dev
    ports: ["5432:5432"]
    volumes: [pgdata:/var/lib/postgresql/data]
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U snackshop"]
      interval: 5s
  mailpit:
    image: axllent/mailpit
    ports: ["8025:8025", "1025:1025"]   # UI 8025, SMTP 1025
volumes:
  pgdata:
```

Backend chạy ngoài Docker khi dev (hot reload nhanh hơn). Chỉ Postgres và Mailpit trong container.

## 3. Cấu hình ứng dụng

Thứ tự ưu tiên (ASP.NET Core mặc định): `appsettings.json` < `appsettings.{Environment}.json` < user-secrets (dev) < **biến môi trường** < command line.

Quy tắc:
- `appsettings.json`: cấu trúc đầy đủ, giá trị **không nhạy cảm**, mặc định an toàn.
- `appsettings.Development.json`: connection string local, log verbose, `Jwt:Secret` dev cố định (chỉ dev).
- Secret thật: **biến môi trường** trong `.env` (prod) hoặc `dotnet user-secrets` (dev). Phân cấp bằng `__`: `ConnectionStrings__Default`, `Jwt__Secret`.
- Mọi section bind qua Options class có `[Required]` + `ValidateOnStart()` → app **không khởi động** nếu thiếu config, thay vì fail lúc runtime.

Danh sách biến (trong `.env.example`):

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Default=Host=postgres;Database=snackshop;Username=snackshop;Password=***
Jwt__Secret=***(>=64 bytes base64)
Jwt__Issuer=snackshop-api
Jwt__Audience=snackshop-web
Jwt__AccessTokenMinutes=15
Jwt__RefreshTokenDays=14
Google__ClientId=***.apps.googleusercontent.com
Email__Host=smtp.resend.com
Email__Port=587
Email__Username=resend
Email__Password=***
Email__From=SnackShop <no-reply@snackshop.vn>
Storage__Provider=Local
Storage__LocalPath=/app/uploads
Storage__PublicBaseUrl=https://api.snackshop.vn/uploads
Cors__AllowedOrigins__0=https://snackshop.vn
Frontend__BaseUrl=https://snackshop.vn
Seed__AdminEmail=admin@snackshop.vn
Seed__AdminPassword=***
Serilog__MinimumLevel__Default=Information
```

## 4. Dockerfile (Api)

File: `backend/src/SnackShop.Api/Dockerfile`. **Build context là thư mục gốc repo** để `.editorconfig` (luật analyzer, warnings-as-errors) áp dụng giống CI:

```bash
docker build -f backend/src/SnackShop.Api/Dockerfile -t snackshop-api .
```

- Stage `build`: restore theo từng `.csproj` (cache layer), publish Release.
- Stage `migrations`: cài `dotnet-ef`, sinh **script SQL idempotent** cho mọi migration (dùng `DesignTimeDbContextFactory`, không cần DB) → `/app/migrations.sql` trong image. `deploy.sh` áp đúng script của phiên bản sắp chạy.
- Stage `runtime`: `aspnet:10.0-alpine`, user không phải root có sẵn của image .NET (`app`, uid 1654), `TZ=Asia/Ho_Chi_Minh`, uploads ở `/app/uploads` (volume), `HEALTHCHECK /health`. Image ~230 MB.

`/.dockerignore` ở gốc repo loại `bin/ obj/ node_modules/ .next/ tests/ docs/ infra/ .env`.

## 5. Logging và quan sát

- **Serilog** sink Console dạng **JSON compact** (`Serilog.Formatting.Compact`). Docker thu stdout; `docker logs` hoặc Loki đọc được.
- Enrich: `TraceId`, `SpanId`, `RequestPath`, `UserId` (nếu có), `Environment`, `Application`.
- `UseSerilogRequestLogging()` thay request log mặc định (1 dòng/request, có status + thời gian).
- Mức mặc định: `Information`; `Microsoft.AspNetCore` → `Warning`; `Microsoft.EntityFrameworkCore.Database.Command` → `Warning` (prod) / `Information` (dev).
- Health checks: `/health` (self), `/health/ready` (Npgsql check). Compose dùng `/health`.
- Phase 3: thêm **Seq** (1 container, miễn phí single-user) hoặc Grafana Loki + Grafana để tìm log theo `traceId`. OpenTelemetry traces khi cần.
- Uptime: UptimeRobot / Better Stack free tier ping `/health` 5 phút/lần, cảnh báo Telegram.

## 6. CI (GitHub Actions)

`.github/workflows/ci.yml` chạy trên mọi PR và push `main`/`develop`:

```
jobs:
  backend:
    - checkout
    - setup-dotnet 10.x
    - dotnet restore (cache NuGet)
    - dotnet format --verify-no-changes
    - dotnet build -c Release --no-restore -warnaserror
    - dotnet test -c Release --no-build --collect:"XPlat Code Coverage"
        (Testcontainers cần Docker: ubuntu-latest có sẵn)
    - dotnet list package --vulnerable --include-transitive → fail nếu High/Critical
    - export openapi.json → upload artifact
    - ef migrations script --idempotent → upload artifact
  frontend:
    - setup-node 22 + pnpm, install (cache)
    - pnpm lint, pnpm typecheck, pnpm test, pnpm build
    - kiểm tra client sinh từ openapi.json khớp (fail nếu diff chưa commit)
  docker (chỉ main/develop):
    - build image api, tag sha + latest, push GHCR
```

Rule bảo vệ branch `main`: CI xanh + 1 review (nếu team ≥ 2) mới merge.

## 7. CD (deploy VPS)

`.github/workflows/deploy.yml` chạy khi push tag `v*` (hoặc chạy tay với một tag có sẵn):

1. Build image API (context gốc repo) → push `ghcr.io/<owner>/<repo>/api:<tag>` và `:latest`.
2. `scp` `infra/docker-compose.prod.yml`, `infra/nginx`, `infra/scripts` lên `/opt/snackshop/infra` (file `.env` **không** bao giờ đi qua CI; nó chỉ nằm trên VPS).
3. SSH chạy `./scripts/deploy.sh <tag>`:
   1. `docker compose pull api` với tag mới.
   2. Đảm bảo postgres chạy, **backup trước deploy** (`backups/*_pre-deploy.dump`).
   3. Lấy `/app/migrations.sql` từ image mới, áp bằng `psql -v ON_ERROR_STOP=1` (idempotent: chạy lại an toàn).
   4. `up -d api` với tag mới, chờ `/health/ready` (tối đa ~2 phút).
   5. Thành công → ghi `.last_good_tag`, bật nginx/certbot/backup, reload nginx.
   6. Thất bại → in log, **tự quay về tag trước** (`.last_good_tag`). Schema không rollback → mọi migration phải tương thích ngược (thêm cột nullable, không xóa/đổi tên trong cùng release).

Secrets GitHub cần: `VPS_HOST`, `VPS_USER` (= `deploy`), `VPS_SSH_KEY` (private key), `VPS_PORT` (tùy chọn). Nên tạo GitHub Environment `production` với required reviewer để deploy cần bấm duyệt.

Rollback tay: `./scripts/deploy.sh rollback`. Deploy một phiên bản cũ: `./scripts/deploy.sh v1.0.3`.

### Deploy lần đầu (checklist)

1. Mua VPS Ubuntu 24.04, trỏ DNS `A api.<domain>` (và `<domain>` nếu tự host web) về IP VPS.
2. Root: `bash server-setup.sh "<ssh-public-key-của-bạn>"` (tạo user `deploy`, Docker, ufw, fail2ban, swap).
3. User `deploy`: copy `infra/` vào `/opt/snackshop/infra`, `cp .env.example .env`, điền giá trị thật, `chmod 600 .env`. Sinh secret: `openssl rand -base64 64 | tr -d '
'`.
4. `docker login ghcr.io` (PAT có quyền `read:packages`) nếu repo private.
5. `./scripts/init-letsencrypt.sh` (thử trước với `STAGING=1`). Nginx chạy được cả khi API chưa có (phân giải upstream lúc request).
6. `./scripts/deploy.sh v1.0.0` → migration + seed admin đầu tiên (`Seed__AdminEmail/Password`). Đăng nhập và **đổi mật khẩu admin ngay**.
7. Cấu hình shop trong admin (`/admin/settings`): tài khoản ngân hàng, BIN (VietQR), phí ship.
8. Tập khôi phục một lần: `./scripts/restore.sh backups/<file>.dump` (giữ DB tạm để xem, không đụng DB thật nếu không xác nhận).

Toàn bộ quy trình 5–7 đã được chạy thử trên máy dev với image build local (`SKIP_PULL=1 ./scripts/deploy.sh local`, `DUMMY_ONLY=1 ./scripts/init-letsencrypt.sh` dùng chứng chỉ tự ký): HTTP→HTTPS 301, API 200 qua nginx, `/health/ready` bị chặn 403, deploy lần 2 giữ nguyên dữ liệu, restore hoán đổi DB thành công.

## 8. VPS production

- Spec khởi điểm: 2 vCPU, 4 GB RAM, 60 GB SSD, Ubuntu 24.04 LTS. (Vultr/DigitalOcean Singapore hoặc nhà cung cấp VN để latency thấp.)
- Cài: Docker Engine + Compose plugin, `ufw` (chỉ 22, 80, 443), `fail2ban`, unattended-upgrades, swap 2 GB.
- User `deploy` thuộc group `docker`, SSH key only, tắt password login.
- Compose prod services: `nginx` (ports 80/443), `api` (nội bộ 8080), `postgres` (nội bộ, volume), `certbot` (renew), `backup` (cron container chạy `backup.sh`).
- Frontend Next.js: ưu tiên **Vercel** (free tier đủ, CDN, không tốn RAM VPS). Nếu muốn tự host → thêm service `web` trong compose.
- Nginx: TLS termination, `proxy_pass` → `api:8080`, `client_max_body_size 6m`, gzip, cache tĩnh `/uploads/` 7 ngày, security headers, rate limit thêm một lớp cho `/api/v1/auth/`.
- Uploads: volume `uploads` mount vào api `/app/uploads`, Nginx phục vụ trực tiếp `alias`. Chuyển S3-compatible (Backblaze B2 / Cloudflare R2) khi > 10 GB hoặc cần CDN.

## 9. Backup và khôi phục

- Container `backup` (image postgres, busybox `crond`) chạy `backup.sh` lúc **03:00 giờ VN** mỗi ngày; `deploy.sh` chạy thêm một bản `pre-deploy`.
- Mỗi lần: `pg_dump -Fc` → `infra/backups/snackshop_<thời gian>_<nhãn>.dump` + `uploads_<thời gian>_<nhãn>.tar.gz`; giữ `BACKUP_RETENTION_DAYS` (mặc định 14) ngày.
- **Sao lưu ra ngoài VPS** (bắt buộc trước go-live): trên host cài `rclone`, cấu hình remote (Backblaze B2 / Cloudflare R2 / Google Drive), thêm crontab của user `deploy`:
  `30 3 * * * rclone copy /opt/snackshop/infra/backups remote:snackshop-backups --max-age 48h`
- `restore.sh <file> [--yes]`: khôi phục vào DB `<db>_restore`, in số dòng `orders/products/users`, hỏi xác nhận, dừng API, đổi tên DB cũ thành `<db>_before_restore_<time>` (giữ lại), đưa bản khôi phục vào, khởi động API. **Tập lại trước go-live và mỗi quý.**

## 10. Runbook ngắn

| Tình huống | Làm gì |
|---|---|
| API 502 | `docker compose logs --tail 200 api`; kiểm `docker compose ps`; thường do config thiếu (`ValidateOnStart` fail) hoặc DB chưa sẵn |
| DB đầy disk | `df -h`; xóa backup cũ local; kiểm `pg_stat_user_tables` bảng `stock_movements`, `refresh_tokens`, `idempotency_keys`; chạy job dọn |
| Deploy hỏng | `deploy.sh rollback` → image tag cũ. Migration đã chạy không rollback tự động; migration phải backward-compatible |
| Lộ secret | Xoay `Jwt__Secret` (mọi user đăng nhập lại), đổi password DB, SMTP, Google secret; kiểm log truy cập |
| Cần xem log 1 request | Lấy `traceId` từ response lỗi → grep `docker compose logs api | grep <traceId>` (hoặc Seq) |
