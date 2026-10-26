# ADR-0007: Minimal APIs thay Controllers

- **Trạng thái**: Accepted
- **Ngày**: 2026-10-22
- **Người quyết định**: Chủ dự án

## Bối cảnh

ASP.NET Core có hai cách viết HTTP API: MVC Controllers và Minimal APIs. Lớp `Api` của ta rất mỏng (bind → send → map Result), không dùng filter/model binding phức tạp của MVC.

## Quyết định

Minimal APIs với pattern `IEndpointGroup` (mỗi module một class, `Map(IEndpointRouteBuilder)`), `MapGroup` cho prefix/tag/policy chung, `TypedResults`/`Results` trả về, metadata OpenAPI qua `.WithName/.Produces`. Tự động đăng ký mọi `IEndpointGroup` bằng reflection một lần lúc start.

## Lựa chọn đã cân nhắc

- **Controllers**: quen thuộc, nhiều tài liệu, `[ApiController]` tự validate ModelState. Nhưng nặng hơn (MVC pipeline), khuyến khích nhét logic vào action, attribute rải rác; validation ta đã làm ở `ValidationBehavior` nên không cần ModelState.
- **FastEndpoints**: hay nhưng thêm framework và cách làm riêng; Minimal APIs của .NET 10 đã đủ (validation, OpenAPI, filters, rate limiting tích hợp).

## Hệ quả

Tốt: endpoint ngắn, nhìn thấy toàn bộ route của một module trong một file; hiệu năng tốt hơn; AOT-ready nếu cần sau.

Đánh đổi: người quen Controllers cần làm quen; một số thư viện bên thứ ba vẫn giả định Controllers (hiếm).

Việc phải làm theo: Architecture test đảm bảo không có `ControllerBase`; feature mẫu Phase 0 là template cho endpoint group.
