# ADR-0001: Clean Architecture monolith với CQRS nhẹ

- **Trạng thái**: Accepted
- **Ngày**: 2026-10-22
- **Người quyết định**: Chủ dự án

## Bối cảnh

Dự án bán đồ ăn vặt cho một shop, team 1–3 người, backend C#. Cần kiến trúc đủ rõ để người mới nắm nhanh, đủ chặt để không rối khi lớn, nhưng không tốn công vận hành. Các lựa chọn: N-layer đơn giản, Clean Architecture monolith, Modular monolith, Microservices.

## Quyết định

Một solution với 4 project `Domain / Application / Infrastructure / Api`, quy tắc phụ thuộc hướng vào trong, use case tổ chức theo vertical slice trong `Application/Features`, CQRS nhẹ (command qua repository + UnitOfWork, query đọc thẳng `IApplicationDbContext` projection), MediatR làm dispatcher, Result pattern cho lỗi nghiệp vụ. Một tiến trình deploy, một DB.

## Lựa chọn đã cân nhắc

- **N-layer (Controller/Service/Repository)**: nhanh nhất lúc đầu, nhưng service class phình to, logic nghiệp vụ lẫn với truy cập dữ liệu, khó test, không có nơi rõ ràng cho invariant.
- **Modular monolith**: ranh giới module chặt (mỗi module có DbContext/schema riêng, giao tiếp qua contract). Tốt cho tương lai tách service nhưng tốn 20–30 % công thiết lập, thừa với 1 shop.
- **Microservices**: không phù hợp team nhỏ, chưa có nhu cầu scale độc lập; chi phí vận hành, debug phân tán rất cao.

## Hệ quả

Tốt: Domain thuần C# test không cần hạ tầng; thay DB/email/storage không chạm nghiệp vụ; mỗi feature là một thư mục, dễ review và bàn giao; đường tách module sau này đã có sẵn (thư mục `Features/*`, `Domain/*`).

Đánh đổi: nhiều file hơn cho mỗi feature (command, handler, validator, response); người mới cần hiểu Result pattern và MediatR; nguy cơ over-engineering nếu thêm pattern vô tội vạ.

Việc phải làm theo: Architecture tests chặn vi phạm phụ thuộc; feature mẫu ở Phase 0 làm template; mọi pattern mới phải có ADR.
