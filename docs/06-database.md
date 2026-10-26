# 06 — Database: PostgreSQL + EF Core

Lý do chọn PostgreSQL: [ADR-0002](adr/0002-postgresql.md).

## 1. Phiên bản và thư viện

| Thành phần | Lựa chọn |
|---|---|
| PostgreSQL | 17 (image `postgres:17-alpine`) |
| EF Core | 10.x |
| Provider | `Npgsql.EntityFrameworkCore.PostgreSQL` 10.x |
| Naming | `EFCore.NamingConventions` → `UseSnakeCaseNamingConvention()` |
| Migration tool | `dotnet-ef` (global tool, version khớp EF Core) |
| Test | `Testcontainers.PostgreSql` + `Respawn` |

Một `ApplicationDbContext` duy nhất. Identity (`IdentityDbContext<ApplicationUser, ApplicationRole, Guid>`) **nằm trong cùng context** để dùng chung transaction và migration.

## 2. Quy ước đặt tên trong DB

Tất cả **snake_case**, tự động bởi `UseSnakeCaseNamingConvention()`. Không viết tên bảng/cột tay trừ khi override có lý do.

| Thứ | Quy tắc | Ví dụ |
|---|---|---|
| Bảng | số nhiều, snake_case | `products`, `order_items`, `stock_movements` |
| Cột | snake_case | `created_at`, `quantity_on_hand` |
| PK | `id` (uuid) | |
| FK | `<entity>_id` | `category_id`, `order_id` |
| Owned type (VO) | `<prop>_<field>` | `Money Price` → `price_amount`, `price_currency`; `ShippingAddress` → `shipping_address_full_name`… |
| Index | `ix_<table>_<cols>` | `ix_products_slug` (tự sinh) |
| Unique | `ux_<table>_<cols>` | Đặt tay trong configuration |
| FK constraint | `fk_<table>_<ref>_<col>` | tự sinh |
| Sequence | `seq_<purpose>` | `seq_order_number_daily` |
| Identity tables | `asp_net_users`, `asp_net_roles`… | Đặt tên tay trong `ApplicationDbContext` (Identity tự đặt PascalCase) |

Enum lưu dạng `varchar(30)` qua `HasConversion<string>()`. Không dùng Postgres enum type (khó migrate khi thêm giá trị).

## 3. Kiểu dữ liệu chuẩn

| C# | PostgreSQL | Ghi chú |
|---|---|---|
| `Guid` (v7) | `uuid` | `Guid.CreateVersion7()` ở app, index B-tree hiệu quả vì tăng theo thời gian |
| `string` | `varchar(n)` với `HasMaxLength(n)` **bắt buộc**; `text` cho mô tả dài | Không để `varchar` không giới hạn |
| `decimal` (Money.Amount) | `numeric(18,2)` | `HasPrecision(18, 2)` |
| `DateTimeOffset` | `timestamptz` | Luôn UTC. **Không** dùng `DateTime` |
| `bool` | `boolean` | |
| `int` | `integer` | |
| `long` | `bigint` | |
| `string` JSON | `jsonb` | `HasColumnType("jsonb")`, chỉ cho payload bên thứ ba |
| `uint RowVersion` | `xid` (`xmin`) | `IsRowVersion()` + `HasColumnName("xmin")` + `HasColumnType("xid")` |

## 4. Trường chuẩn trên entity

### Audit (mọi aggregate root và entity có vòng đời riêng)

```csharp
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }
    Guid? CreatedBy { get; }
    DateTimeOffset? UpdatedAt { get; }
    Guid? UpdatedBy { get; }
}
```

`AuditableEntityInterceptor` (`SaveChangesInterceptor`) set giá trị dựa trên `TimeProvider` và `ICurrentUser`. Không set tay trong handler.

### Soft delete (chỉ `Category`, `Product`)

`ISoftDeletable { bool IsDeleted; DateTimeOffset? DeletedAt; }` + `HasQueryFilter(e => !e.IsDeleted)`. Interceptor chuyển `Remove()` thành update `IsDeleted = true`. Admin xem thùng rác dùng `IgnoreQueryFilters()`.

`Order`, `Payment`, `StockMovement`, `OrderStatusHistory`: **không bao giờ xóa**, không có `IsDeleted`.

### Concurrency

`StockItem`, `Order`, `Cart` có `xmin` làm row version. `DbUpdateConcurrencyException` → `UnitOfWorkBehavior` map thành `Error.Conflict("Concurrency.Conflict", ...)`, client retry.

## 5. Cấu hình entity (Fluent API)

Mỗi entity một file trong `Infrastructure/Persistence/Configurations/`:

```csharp
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.HasKey(p => p.Id);

        b.Property(p => p.Name).HasMaxLength(200).IsRequired();

        b.Property(p => p.Slug)
            .HasConversion(s => s.Value, v => Slug.FromValue(v))
            .HasMaxLength(120)
            .IsRequired();
        b.HasIndex(p => p.Slug).IsUnique().HasDatabaseName("ux_products_slug");

        b.HasMany(p => p.Variants).WithOne().HasForeignKey(v => v.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(p => p.Images).WithOne().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(p => p.Variants).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(p => p.Images).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasOne<Category>().WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);

        b.HasQueryFilter(p => !p.IsDeleted);
        b.HasIndex(p => new { p.CategoryId, p.IsActive });
    }
}
```

Quy tắc:
- `ApplyConfigurationsFromAssembly` trong `OnModelCreating`. Không cấu hình trong `OnModelCreating` trực tiếp.
- Value object `Money`: `b.OwnsOne(v => v.Price, m => { m.Property(x => x.Amount).HasPrecision(18,2); m.Property(x => x.Currency).HasMaxLength(3); })`.
- Quan hệ xuyên aggregate (`Product → Category`, `OrderItem → Variant`): **không navigation property**, chỉ `HasOne<T>().WithMany().HasForeignKey(...)` để có FK constraint nhưng Domain không tham chiếu.
- `DeleteBehavior.Cascade` chỉ trong cùng aggregate. Xuyên aggregate: `Restrict`.
- Ngoại lệ FK: `order_items.variant_id` **không** có FK constraint để admin có thể xóa cứng variant lỗi mà không hỏng đơn cũ (đơn đã snapshot). Ghi rõ trong configuration bằng comment.

## 6. Index cần có từ đầu

| Bảng | Index | Lý do |
|---|---|---|
| `products` | `ux_products_slug`; `(category_id, is_active)`; `(is_featured) where is_active`; GIN trên `to_tsvector('simple', name)` (Phase 2, qua migration raw SQL) | Lookup slug, lọc, search |
| `product_variants` | `ux_product_variants_sku`; `(product_id)` | |
| `categories` | `ux_categories_slug` | |
| `orders` | `ux_orders_order_number`; `(customer_id, placed_at desc)`; `(status, placed_at desc)`; `(shipping_address_phone)` | Lịch sử, admin filter, tra cứu guest |
| `order_items` | `(order_id)`; `(variant_id)` | Top products report |
| `stock_movements` | `(variant_id, created_at desc)` | |
| `payments` | `ux_payments_order_id`; `(status)` | |
| `refresh_tokens` | `ux_refresh_tokens_token_hash`; `(user_id)` | |
| `orders` | `ix_orders_idempotency_key` unique (filtered `IS NOT NULL`) | Idempotency đặt hàng (ADR-0009) |

Search tiếng Việt MVP: `LIKE '%term%'` trên cột `search_text` (đã bỏ dấu, do Domain tính) — không cần extension. Khi chậm → index `pg_trgm` trên `search_text`.

## 7. Sinh `OrderNumber`

Format `SS-yyyyMMdd-NNNNN`. Sequence Postgres `seq_order_number` (bigint, không reset) → lấy `nextval` trong `OrderRepository.NextOrderNumberAsync()`, `NNNNN = value % 100000` padding 5 số. Đơn giản, không khóa bảng, không trùng. Tạo sequence bằng migration `migrationBuilder.CreateSequence<long>("seq_order_number")`.

## 8. Migration workflow

`DesignTimeDbContextFactory` (Infrastructure) cho phép mọi lệnh `dotnet ef` chạy mà không boot API, không cần biến môi trường hay secret. Chỉ `database update` mới kết nối DB (mặc định DB dev local, đổi bằng biến `SNACKSHOP_DESIGN_CONNECTION`).

```bash
# Tạo migration (chạy từ backend/)
dotnet ef migrations add <Verb><Noun> \
  --project src/SnackShop.Infrastructure \
  --startup-project src/SnackShop.Api \
  --output-dir Persistence/Migrations

# Áp dụng local
dotnet ef database update --project src/SnackShop.Infrastructure --startup-project src/SnackShop.Api

# Sinh script idempotent cho production
dotnet ef migrations script --idempotent -o infra/migrations/$(date +%Y%m%d).sql \
  --project src/SnackShop.Infrastructure --startup-project src/SnackShop.Api
```

Quy tắc:
1. Tên migration: `PascalCase`, mô tả thay đổi: `AddPaymentsTable`, `AddSoldCountToProducts`. Không `Update1`, `Fix`.
2. **Một PR, một migration** (gộp nếu đang dev local chưa push: xóa và tạo lại).
3. **Không sửa migration đã merge vào `main`.** Tạo migration mới để sửa.
4. Review file migration trong PR, không chỉ snapshot. Đặc biệt: `DropColumn`, `AlterColumn` đổi kiểu, `DeleteBehavior`.
5. Migration làm mất dữ liệu phải có bước copy dữ liệu bằng `migrationBuilder.Sql()` và được đánh dấu `// DATA MIGRATION` trong PR description.
6. Áp migration lên production **bằng script SQL idempotent đóng gói trong image API** (`/app/migrations.sql`, sinh lúc `docker build`); `infra/scripts/deploy.sh` chạy nó bằng `psql` trước khi khởi động container mới. **Không** `Database.Migrate()` lúc app start ở production. Development được phép `Migrate()` khi start.
7. Mọi migration phải chạy được trên DB rỗng từ đầu (integration test làm việc này).

## 9. Seed data

`DbSeeder` trong Infrastructure, chạy khi `app.Environment.IsDevelopment()` hoặc qua lệnh `dotnet run -- --seed`:

- **Luôn** (mọi môi trường, idempotent): roles `Customer`, `Staff`, `Admin`; user admin đầu tiên từ biến môi trường `Seed__AdminEmail` / `Seed__AdminPassword` (chỉ tạo nếu chưa có); `ShopSettings` row mặc định.
- **Chỉ Development**: 5 category, 30 product với variant/image placeholder, stock 100, 20 đơn mẫu đủ các trạng thái, 5 customer.

Dữ liệu seed dev dùng Bogus với seed cố định để deterministic.

## 10. Hiệu năng và vận hành

- Connection string: `Pooling=true;Minimum Pool Size=2;Maximum Pool Size=50`. Dùng `NpgsqlDataSource` qua `AddNpgsqlDataSource`.
- `AddDbContextPool<ApplicationDbContext>` không dùng vì có interceptor phụ thuộc scoped `ICurrentUser`; dùng `AddDbContext` thường.
- `EnableSensitiveDataLogging()` và `LogTo` chỉ Development.
- Query phải xem được SQL khi review: bật `Microsoft.EntityFrameworkCore.Database.Command` ở `Information` trong `appsettings.Development.json`.
- Tránh N+1: query luôn projection; nếu buộc load aggregate để ghi → `Include` tường minh trong repository, có `AsSplitQuery()` khi ≥ 2 collection.
- Transaction: mặc định `SaveChanges` là 1 transaction. Không mở transaction tay trong handler. Nếu một use case cần nhiều `SaveChanges` → xem lại thiết kế.
- Backup: `pg_dump` hằng ngày lúc 03:00 giờ VN, giữ 14 bản, cấu hình trong [08-infrastructure-devops.md](08-infrastructure-devops.md).

## 11. Những điều cấm

- `DateTime` kiểu cột `timestamp` (không timezone).
- `string` không `HasMaxLength` (trừ cột cố ý là `text`).
- Cascade delete xuyên aggregate.
- Lazy loading (`UseLazyLoadingProxies`). Không bao giờ.
- `Database.EnsureCreated()`. Chỉ migration.
- Raw SQL nối chuỗi.
- Đặt logic trong trigger / stored procedure. Logic ở Domain.
