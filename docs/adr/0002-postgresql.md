# ADR-0002: PostgreSQL làm database chính

- **Trạng thái**: Accepted
- **Ngày**: 2026-10-22
- **Người quyết định**: Chủ dự án

## Bối cảnh

Cần một RDBMS cho dữ liệu giao dịch (đơn hàng, kho) với EF Core, chạy tốt trong Docker trên VPS Linux giá rẻ, không phát sinh license. Ứng viên: PostgreSQL, SQL Server, MySQL/MariaDB.

## Quyết định

PostgreSQL 17, provider `Npgsql.EntityFrameworkCore.PostgreSQL`, naming snake_case, Id `uuid` v7, tiền `numeric(18,2)`, thời gian `timestamptz`, concurrency bằng `xmin`.

## Lựa chọn đã cân nhắc

- **SQL Server**: tooling .NET quen thuộc (SSMS), nhưng Express giới hạn 10 GB/1 socket, bản đầy đủ tốn license; image Docker nặng (~1.5 GB) và ngốn RAM trên VPS 4 GB.
- **MySQL/MariaDB**: phổ biến ở hosting VN, nhưng provider EF Core (Pomelo) là cộng đồng, ít tính năng hơn Npgsql; thiếu `jsonb`, CTE/window function yếu hơn, không có `xmin` tiện cho concurrency.
- **PostgreSQL**: miễn phí hoàn toàn, Npgsql do chính team EF Core hỗ trợ, `jsonb`, full-text, `unaccent` cho tiếng Việt, image alpine nhẹ, mọi cloud đều có managed Postgres rẻ khi cần.

## Hệ quả

Tốt: chi phí 0, nhẹ, tính năng đủ cho search/report sau này, dễ chuyển sang managed DB.

Đánh đổi: team quen SQL Server phải học `psql`/pgAdmin và vài khác biệt (`ILIKE`, không có `TOP`, timezone chặt hơn).

Việc phải làm theo: luôn `DateTimeOffset`/`timestamptz`; migration chạy bằng SQL script ở production; backup `pg_dump` hằng ngày.
