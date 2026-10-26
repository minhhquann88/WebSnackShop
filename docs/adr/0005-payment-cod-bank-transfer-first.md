# ADR-0005: COD + chuyển khoản VietQR trước, abstraction cho cổng thanh toán

- **Trạng thái**: Accepted
- **Ngày**: 2026-10-22
- **Người quyết định**: Chủ dự án

## Bối cảnh

Đồ ăn vặt giá trị đơn thấp, khách Việt Nam quen COD và chuyển khoản QR. Tích hợp VNPay/MoMo cần đăng ký merchant, xử lý IPN/return URL, sandbox, mất 1–2 tuần và có thể chặn tiến độ MVP.

## Quyết định

MVP hỗ trợ hai phương thức: `Cod` (thanh toán khi nhận, `Payment.Status = Paid` khi `Delivered`) và `BankTransfer` (hiện VietQR + nội dung chuyển khoản chuẩn `SS yyyyMMdd NNNNN`, staff xác nhận tay, hệ thống tự hủy đơn sau 24h chưa thanh toán). Thiết kế sẵn `IPaymentGateway` trong Application, bảng `payments` có `Provider`, `TransactionRef`, `RawPayload jsonb` để cắm cổng online sau mà không đổi schema.

## Lựa chọn đã cân nhắc

- **VNPay/MoMo ngay từ đầu**: trải nghiệm tốt hơn, tự động hơn, nhưng chậm MVP, thêm rủi ro bảo mật callback khi chưa có kinh nghiệm.
- **Chỉ COD**: đơn giản nhất nhưng bỏ lỡ nhóm khách muốn trả trước, và shop không chống được "bom hàng".
- **COD + chuyển khoản**: phủ ~90 % nhu cầu khách VN, không phí giao dịch, làm trong 1–2 ngày.

## Hệ quả

Tốt: ra mắt nhanh; không phụ thuộc bên thứ ba; schema và interface đã sẵn cho Phase 4.

Đánh đổi: staff phải đối soát chuyển khoản tay (giảm bằng nội dung CK chuẩn + QR); đơn CK chưa thanh toán giữ kho tối đa 24h.

Việc phải làm theo: Phase 4 thêm `VnPayGateway`/`MoMoGateway` + endpoint IPN; cân nhắc dịch vụ đọc giao dịch ngân hàng (Casso/SePay) để tự động xác nhận CK trước khi làm cổng.
