# ADR-0008: Giữ hàng (reserve) ngay khi đặt đơn

- **Trạng thái**: Accepted
- **Ngày**: 2026-10-22
- **Người quyết định**: Chủ dự án

## Bối cảnh

Đồ ăn vặt thường nhập số lượng nhỏ, dễ hết. Nếu chỉ trừ kho khi staff xác nhận, hai khách có thể cùng đặt món cuối cùng, một người sẽ bị hủy sau. Nếu trừ thẳng kho khi đặt, đơn chuyển khoản không bao giờ thanh toán sẽ "khóa" hàng vô thời hạn và báo cáo tồn sai.

## Quyết định

`StockItem` có hai số: `QuantityOnHand` và `QuantityReserved`; bán được = hiệu của hai số. Đặt đơn → `Reserve` (fail nếu không đủ, đơn không tạo). Xác nhận → `CommitReservation` (trừ OnHand, giảm Reserved, movement `Sale`). Hủy từ Pending → `ReleaseReservation`. Hủy sau xác nhận → `Return`. Đơn chuyển khoản quá 24h chưa thanh toán → job tự hủy và release. Concurrency bằng `xmin` trên `StockItem`; xung đột → retry hoặc trả 409.

## Lựa chọn đã cân nhắc

- **Trừ kho khi xác nhận**: đơn giản, nhưng oversell khi nhiều khách đặt gần nhau; staff phải gọi xin lỗi.
- **Trừ kho khi đặt, cộng lại khi hủy**: không phân biệt được "có trong kho" và "bán được", báo cáo nhập xuất tồn sai, đơn treo giữ hàng mãi.
- **Reserve/commit/release**: thêm một cột và vài method, nhưng đúng nghiệp vụ, kho luôn khớp, có lịch sử rõ.

## Hệ quả

Tốt: không oversell; admin thấy cả "thực có" và "đang giữ"; đơn treo tự giải phóng.

Đánh đổi: mỗi chuyển trạng thái đơn phải chạm kho đúng bảng trong docs/03; cần test song song để chắc concurrency; staff hủy đơn ở trạng thái `Shipping` phải chỉnh kho tay.

Việc phải làm theo: unit test từng method của `StockItem`; integration test 2 request đặt hàng song song cho món còn 1; job hết hạn chuyển khoản ở Phase 2.
