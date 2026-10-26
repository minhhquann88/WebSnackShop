# CLAUDE.md — Hướng dẫn cho AI agent làm việc trong repo này

Repo: **SnackShop**, web bán đồ ăn vặt. Backend C#/.NET 10 Clean Architecture, PostgreSQL, Next.js frontend. Monorepo.

## Đọc trước khi làm bất cứ gì

1. [docs/02-architecture.md](docs/02-architecture.md) — 4 lớp, quy tắc phụ thuộc, CQRS nhẹ, nơi đặt code.
2. [docs/04-coding-conventions.md](docs/04-coding-conventions.md) — naming, Result pattern, điều cấm.
3. [docs/03-domain-model.md](docs/03-domain-model.md) — khi chạm nghiệp vụ (đơn hàng, kho, catalog).
4. [docs/05-api-guidelines.md](docs/05-api-guidelines.md) — khi thêm/sửa endpoint.
5. [docs/06-database.md](docs/06-database.md) — khi đổi entity/migration.

Tài liệu là nguồn sự thật. Nếu code và docs lệch nhau, hỏi hoặc sửa cả hai trong cùng PR.

## Quy tắc cứng (không có ngoại lệ)

- Business logic ở `Domain`. Handler ≤ 60 dòng. Endpoint chỉ bind → send → map Result.
- Lỗi nghiệp vụ trả `Result.Failure(Error)` với mã `Module.Thing.Reason`. Không throw cho lỗi nghiệp vụ.
- Command dùng repository; query dùng `IApplicationDbContext` + `AsNoTracking` + `Select` projection. Mọi list có phân trang.
- Entity: setter `private`, collection `IReadOnlyCollection`, Id `Guid.CreateVersion7()`, thời gian qua `TimeProvider`.
- Không AutoMapper, không generic repository, không lazy loading, không `DateTime.Now`, không magic string.
- Mỗi feature mới = Command/Query + Handler + Validator + Response trong `Application/Features/<Module>/<UseCase>/`, kèm unit test Domain (nếu có invariant) và integration test endpoint (happy + failure + auth).
- Không sửa migration đã merge. Tạo migration mới.
- Không commit secret. Không log token/mật khẩu/SĐT đầy đủ.
- Đổi quyết định kiến trúc → viết ADR trong `docs/adr/`.

## Lệnh thường dùng (sau khi scaffold Phase 0)

```bash
docker compose -f infra/docker-compose.yml up -d
cd backend && dotnet build && dotnet test
dotnet run --project src/SnackShop.Api
dotnet ef migrations add <Name> --project src/SnackShop.Infrastructure --startup-project src/SnackShop.Api --output-dir Persistence/Migrations
dotnet format
cd frontend && pnpm install && pnpm dev && pnpm gen:api
```

## Khi được giao task

- Đọc feature mẫu `ShopSettings` (Phase 0.10) và copy cấu trúc.
- Nếu task mơ hồ về nghiệp vụ, kiểm tra `docs/03` trước; nếu vẫn mơ hồ, hỏi, đừng đoán rồi viết nhiều code.
- Trước khi báo xong: `dotnet build` 0 warning, `dotnet test` xanh, `dotnet format --verify-no-changes` pass.
- Commit theo Conventional Commits (`feat(orders): ...`), xem `docs/10-git-workflow.md`.

## Trạng thái hiện tại

Phase 0–3 của [docs/12-roadmap.md](docs/12-roadmap.md) đã hiện thực: Catalog, Inventory, Customers (sổ địa chỉ, giỏ hàng), Ordering (COD + chuyển khoản VietQR, state machine, giữ hàng), Auth (Identity + JWT + refresh rotation + Google), Admin (đơn, kho, khách, nhân viên, báo cáo), email, job nền, hạ tầng production (`infra/`). Frontend Next.js ở `frontend/`. Các điểm lệch so với thiết kế ban đầu: [ADR-0009](docs/adr/0009-mvp-implementation-choices.md). Việc còn lại: mục ⬜/🟡 trong roadmap.

Bản đồ code:
- Feature mẫu đơn giản: `Application/Features/Settings/*` + `Api/Endpoints/Admin/AdminSettingsEndpoints.cs`.
- Feature mẫu đầy đủ (domain + kho + email): `Application/Features/Orders/*`, `Domain/Ordering/Order.cs`.
- Helper dùng chung giữa handler: đăng ký trong `Application/DependencyInjection.cs` (vd. `OrderInventory`, `CustomerProfiles`, `PurchasableVariants`).
- Test tích hợp: `tests/SnackShop.Api.IntegrationTests` dùng `TestApi` (đăng ký khách thật, đăng nhập admin seed, tạo sản phẩm qua API).

Lưu ý môi trường:
- `dotnet ef` dùng `DesignTimeDbContextFactory` (không cần biến môi trường); chỉ `database update` kết nối DB dev local.
- `dotnet test` dùng Microsoft.Testing.Platform (cấu hình trong `backend/global.json`); integration test cần Docker đang chạy.
- Test method không cần hậu tố `Async`; mọi method async khác bắt buộc có.
- Docker image API build từ **gốc repo**: `docker build -f backend/src/SnackShop.Api/Dockerfile .`
- Đổi API → export `backend/openapi.json` rồi `cd frontend && pnpm gen:api` (CI kiểm tra cả hai).
