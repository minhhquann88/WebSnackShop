# SnackShop — Web bán đồ ăn vặt

Nền tảng bán đồ ăn vặt trực tuyến gồm **web cho khách hàng** (xem menu, đặt hàng, theo dõi đơn) và **trang quản trị** (quản lý sản phẩm, đơn hàng, kho, báo cáo).

> Thư mục [`docs/`](docs/) là **nguồn sự thật** về kiến trúc và quy tắc viết code của dự án.
> Người mới vào team đọc theo thứ tự số (01 → 12) trước khi viết dòng code đầu tiên.
> Mọi thay đổi về kiến trúc phải đi kèm một ADR trong [`docs/adr/`](docs/adr/).

## Stack đã chốt

| Thành phần | Lựa chọn | Ghi chú |
|---|---|---|
| Backend | C# / .NET 10 (LTS), ASP.NET Core Web API | Minimal APIs, Clean Architecture, CQRS nhẹ |
| ORM | EF Core 10 + Npgsql | Code-first migrations, snake_case |
| Database | PostgreSQL 17 | Chạy trong Docker |
| Xác thực | ASP.NET Core Identity + JWT (access + refresh) + Google OAuth | Roles: `Customer` / `Staff` / `Admin` |
| Frontend | Next.js (App Router) + TypeScript | Thư mục `frontend/`, gọi API qua client sinh từ OpenAPI |
| Thanh toán MVP | COD, chuyển khoản VietQR | Có `IPaymentGateway` để cắm VNPay / MoMo sau |
| Hạ tầng | Docker Compose (local) → VPS Linux + Docker + Nginx (prod) | GitHub Actions CI/CD |
| Logging | Serilog → console (JSON) | Seq / Grafana Loki ở giai đoạn sau |

## Cấu trúc repo (monorepo)

```
Web/
├── README.md                 ← bạn đang ở đây
├── CLAUDE.md                 ← quy tắc cho AI agent / trợ lý code
├── .editorconfig             ← quy tắc format C# (bắt buộc, IDE tự áp dụng)
├── .gitignore
├── docs/                     ← tài liệu kiến trúc & quy tắc (nguồn sự thật)
│   ├── 01-overview.md
│   ├── 02-architecture.md
│   ├── 03-domain-model.md
│   ├── 04-coding-conventions.md
│   ├── 05-api-guidelines.md
│   ├── 06-database.md
│   ├── 07-auth-security.md
│   ├── 08-infrastructure-devops.md
│   ├── 09-testing.md
│   ├── 10-git-workflow.md
│   ├── 11-frontend-guidelines.md
│   ├── 12-roadmap.md
│   └── adr/                  ← Architecture Decision Records
├── backend/                  ← solution C# (.NET 10)
│   ├── SnackShop.slnx
│   ├── src/
│   │   ├── SnackShop.Domain/
│   │   ├── SnackShop.Application/
│   │   ├── SnackShop.Infrastructure/
│   │   └── SnackShop.Api/
│   └── tests/
│       ├── SnackShop.Domain.Tests/
│       ├── SnackShop.Application.Tests/
│       ├── SnackShop.Api.IntegrationTests/
│       └── SnackShop.ArchitectureTests/
│   └── openapi.json          ← hợp đồng API (sinh từ code, frontend dùng để sinh client)
├── frontend/                 ← Next.js app (storefront + /admin)
├── infra/                    ← docker-compose dev/prod, nginx, scripts deploy/backup/restore
└── .github/                  ← CI (ci.yml), CD (deploy.yml), Dependabot, PR template
```

## Bắt đầu nhanh

Yêu cầu: .NET 10 SDK, Docker Desktop, `dotnet tool install -g dotnet-ef --version "10.*"`.

```bash
# 1. Chạy hạ tầng local (Postgres :5432, Mailpit UI :8025)
docker compose -f infra/docker-compose.yml up -d

# 2. Backend (Development tự migrate + seed khi khởi động)
cd backend
dotnet restore
dotnet run --project src/SnackShop.Api
# → http://localhost:5000/scalar   (OpenAPI UI)
# → admin seed: admin@snackshop.local / Admin12345  (appsettings.Development.json)
# → dữ liệu mẫu: 5 danh mục, 14 sản phẩm (chỉ Development)
# → email dev: http://localhost:8025 (Mailpit)

# Kiểm tra chất lượng (giống CI)
dotnet build            # 0 warning
dotnet format --verify-no-changes
dotnet test             # cần Docker cho integration test

# Xuất hợp đồng API (commit file này; CI báo lỗi nếu lệch với code)
dotnet run --project src/SnackShop.Api -- --export-openapi "$(pwd)/openapi.json"

# Tạo migration mới (không cần biến môi trường: dùng DesignTimeDbContextFactory)
dotnet ef migrations add <Name> \
  --project src/SnackShop.Infrastructure --startup-project src/SnackShop.Api --output-dir Persistence/Migrations

# 3. Frontend
cd frontend
pnpm install
pnpm dev
# → http://localhost:3000
```

## Deploy production

Xem [docs/08 §7 "Deploy lần đầu"](docs/08-infrastructure-devops.md#deploy-lần-đầu-checklist): `server-setup.sh` → `.env` → `init-letsencrypt.sh` → `deploy.sh <tag>`. Sau đó mỗi lần push tag `v*`, GitHub Actions build image và deploy tự động (có backup trước deploy và tự rollback nếu API không lên).

## Tài liệu

| # | File | Nội dung |
|---|---|---|
| 01 | [Tổng quan](docs/01-overview.md) | Mục tiêu, người dùng, phạm vi MVP, ngoài phạm vi |
| 02 | [Kiến trúc](docs/02-architecture.md) | Clean Architecture, 4 project, luồng request, quy tắc phụ thuộc |
| 03 | [Domain model](docs/03-domain-model.md) | Entities, aggregates, state machine đơn hàng, ERD, thuật ngữ |
| 04 | [Quy tắc code C#](docs/04-coding-conventions.md) | Naming, cấu trúc feature, Result pattern, validation, điều cấm |
| 05 | [API guidelines](docs/05-api-guidelines.md) | REST, ProblemDetails, phân trang, versioning, danh sách endpoint |
| 06 | [Database](docs/06-database.md) | PostgreSQL + EF Core, naming, migration, audit, concurrency |
| 07 | [Auth & Security](docs/07-auth-security.md) | Identity, JWT, refresh token, Google login, checklist bảo mật |
| 08 | [Hạ tầng & DevOps](docs/08-infrastructure-devops.md) | Docker, cấu hình, logging, CI/CD, deploy VPS, backup |
| 09 | [Testing](docs/09-testing.md) | Chiến lược test, công cụ, naming, coverage |
| 10 | [Git workflow](docs/10-git-workflow.md) | Branch, commit convention, PR, review |
| 11 | [Frontend](docs/11-frontend-guidelines.md) | Cấu trúc Next.js, API client, state, auth phía client |
| 12 | [Roadmap](docs/12-roadmap.md) | Kế hoạch theo phase, task breakdown, Definition of Done |
| — | [ADR](docs/adr/README.md) | Lý do của từng quyết định kiến trúc |
