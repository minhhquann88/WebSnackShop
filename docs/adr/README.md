# Architecture Decision Records

Mỗi quyết định kiến trúc quan trọng được ghi lại thành một file để người sau hiểu **tại sao** chứ không chỉ **cái gì**. ADR không bị sửa sau khi chấp nhận; nếu đổi ý → viết ADR mới và đánh dấu ADR cũ `Superseded by ADR-XXXX`.

## Khi nào cần ADR

- Chọn / thay framework, thư viện cốt lõi, database, cách deploy.
- Thay đổi quy tắc phụ thuộc giữa các lớp.
- Thêm pattern mới áp dụng toàn codebase (Outbox, CQRS tách DB, event sourcing…).
- Quyết định nghiệp vụ ảnh hưởng kiến trúc (guest checkout, reserve kho…).

Không cần ADR cho: đặt tên, chọn thư viện tiện ích nhỏ, sửa bug.

## Danh sách

| ADR | Tiêu đề | Trạng thái | Ngày |
|---|---|---|---|
| [0001](0001-clean-architecture-monolith.md) | Clean Architecture monolith với CQRS nhẹ | Accepted | 2026-10-22 |
| [0002](0002-postgresql.md) | PostgreSQL làm database chính | Accepted | 2026-10-22 |
| [0003](0003-identity-jwt.md) | ASP.NET Core Identity + JWT + refresh rotation + Google | Accepted | 2026-10-22 |
| [0004](0004-monorepo-docker-vps.md) | Monorepo, Docker Compose, deploy VPS | Accepted | 2026-10-22 |
| [0005](0005-payment-cod-bank-transfer-first.md) | COD + chuyển khoản trước, abstraction cho cổng thanh toán | Accepted | 2026-10-22 |
| [0006](0006-mediatr-v12-no-automapper.md) | Pin MediatR 12.x, không dùng AutoMapper, Shouldly thay FluentAssertions | Accepted | 2026-10-22 |
| [0007](0007-minimal-apis.md) | Minimal APIs thay Controllers | Accepted | 2026-10-22 |
| [0008](0008-stock-reservation-on-place.md) | Giữ hàng ngay khi đặt đơn | Accepted | 2026-10-22 |
| [0009](0009-mvp-implementation-choices.md) | Lựa chọn khi hiện thực MVP (search, idempotency, email queue, migration trong image…) | Accepted | 2026-10-26 |

## Template

```markdown
# ADR-XXXX: <Tiêu đề ngắn>

- **Trạng thái**: Proposed | Accepted | Deprecated | Superseded by ADR-YYYY
- **Ngày**: YYYY-MM-DD
- **Người quyết định**: 

## Bối cảnh
Vấn đề gì cần giải quyết, ràng buộc nào.

## Quyết định
Chọn gì. Một đoạn.

## Lựa chọn đã cân nhắc
- A: ưu / nhược
- B: ưu / nhược

## Hệ quả
Tốt: ...
Xấu / đánh đổi: ...
Việc phải làm theo: ...
```
