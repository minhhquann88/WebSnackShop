# ADR-0006: Pin MediatR 12.x, không AutoMapper, Shouldly thay FluentAssertions

- **Trạng thái**: Accepted
- **Ngày**: 2026-10-22
- **Người quyết định**: Chủ dự án

## Bối cảnh

Năm 2025 một số thư viện .NET phổ biến chuyển sang license thương mại: MediatR và AutoMapper từ v13/v15 (Lucky Penny Software), FluentAssertions từ v8 (Xceed). Các bản trước vẫn Apache 2.0. Dự án cần tránh rủi ro license và tránh phụ thuộc không cần thiết.

## Quyết định

- **MediatR pin `12.x`** (Apache 2.0) trong `Directory.Packages.props`; Dependabot chỉ cập nhật patch. Nếu cần rời MediatR, abstraction `ICommand/IQuery/ICommandHandler` của ta đã bọc `IRequest`, nên chuyển sang dispatcher tự viết hoặc Wolverine là thay một lớp mỏng.
- **Không dùng AutoMapper.** Mapping viết tay: `ToResponse()` static hoặc `.Select()` projection. Rõ ràng, debug được, EF dịch sang SQL được, compile-time safe.
- **Shouldly** cho assertion (BSD), thay FluentAssertions.

## Lựa chọn đã cân nhắc

- MediatR v13+ với license community (miễn phí dưới ngưỡng doanh thu): rủi ro điều khoản đổi; không đáng khi v12 đủ dùng.
- Tự viết dispatcher ngay từ đầu: ~100 dòng, nhưng mất pipeline behavior và notification sẵn có; để dành nếu cần.
- Mapperly (source generator, MIT): tốt, nhưng mapping tay ở quy mô này đơn giản hơn và không thêm công cụ.
- FluentAssertions 7.x pin: được, nhưng Shouldly đang phát triển tích cực và cú pháp tương đương.

## Hệ quả

Tốt: toàn bộ dependency core là OSS permissive; mapping tường minh.

Đánh đổi: không nhận tính năng MediatR mới; viết mapping tay tốn vài dòng mỗi DTO.

Việc phải làm theo: Architecture test chặn namespace `AutoMapper`; review Dependabot PR cho MediatR không vượt major 12.
