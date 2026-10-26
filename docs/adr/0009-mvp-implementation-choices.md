# ADR-0009: Lựa chọn khi hiện thực MVP (Phase 1–3)

- **Trạng thái**: Accepted
- **Ngày**: 2026-10-26
- **Người quyết định**: Chủ dự án (đề xuất bởi đội phát triển khi code Phase 1–3)

## Bối cảnh

Khi hiện thực Catalog, Auth, Ordering, Inventory và hạ tầng, một số chi tiết trong docs/02, 03, 05, 06, 08 tỏ ra phức tạp hơn cần thiết hoặc không chạy được như mô tả. ADR này ghi lại các điểm lệch để tài liệu và code khớp nhau.

## Quyết định

1. **Tìm kiếm không dấu bằng cột `products.search_text`** (tên + thương hiệu đã bỏ dấu, chữ thường, do Domain tính trong `TextNormalizer`), truy vấn bằng `LIKE`. Không dùng extension `unaccent` vì Application bị cấm tham chiếu Npgsql (architecture test). Khi cần nhanh hơn: thêm index `pg_trgm` trên cột này, không đổi code.
2. **Tạo `StockItem` ngay trong handler** tạo sản phẩm / thêm biến thể (cùng transaction), thay vì qua domain event. Domain event chạy *sau* commit nên không thể đảm bảo mọi biến thể đều có bản ghi kho.
3. **Idempotency đặt hàng bằng cột `orders.idempotency_key` (unique, nullable)** thay vì bảng `idempotency_keys` riêng. Đơn giản, cùng transaction với đơn, không cần job dọn.
4. **Gộp giỏ hàng khách vãng lai: `POST /me/cart/merge`** (cộng dồn, bỏ qua biến thể không còn bán) thay cho `PUT /me/cart` (thay toàn bộ) để không làm mất món khách đã thêm ở thiết bị khác.
5. **Email qua hàng đợi in-process** (`Channel` + `EmailDispatcher` BackgroundService, thử lại 3 lần). Request không bao giờ chờ SMTP. Email trong hàng đợi mất nếu process khởi động lại — chấp nhận ở MVP; Outbox vẫn là kế hoạch Phase 4.
6. **Domain event handler lỗi sau commit chỉ được log**, không biến request thành 500 (`DispatchDomainEventsInterceptor`).
7. **Ảnh QR chuyển khoản dùng dịch vụ công khai `img.vietqr.io`** (chuẩn NAPAS) thay vì tự sinh bằng QRCoder. Không thêm thư viện, ảnh nhúng được trong email. Nội dung chuyển khoản là mã đơn bỏ dấu gạch: `SS2026102200042` (ngân hàng thường xóa ký tự đặc biệt).
8. **Chưa dùng `HybridCache` cho catalog.** Truy vấn catalog đã projection + index; cache thêm độ phức tạp invalidation khi chưa có số liệu tải. Bật khi p95 vượt mục tiêu.
9. **Migration production = script SQL idempotent đóng gói trong image API** (`/app/migrations.sql`, sinh lúc build bằng `IDesignTimeDbContextFactory`), `deploy.sh` áp script đó trước khi khởi động container mới. Code và schema luôn cùng phiên bản; công cụ EF không cần boot host hay biết secret.
10. **Bảng Identity đặt tên snake_case** (`asp_net_users`…) cho thống nhất với docs/06.
11. **JSON sai định dạng trả 400 `Request.Malformed`** ở mọi môi trường (`RouteHandlerOptions.ThrowOnBadRequest = true` + `GlobalExceptionHandler`), thay vì 400 rỗng ngoài Development.
12. **Docker build context là thư mục gốc repo** để `.editorconfig` (luật analyzer, warnings-as-errors) áp dụng giống hệt CI.

### Bổ sung sau review độc lập (2026-10-27)

13. **Google không tự liên kết vào tài khoản mật khẩu chưa xác nhận email** (`Auth.UnconfirmedAccountExists`) — chống chiếm tài khoản bằng cách đăng ký trước email của người khác. Kiểm tra khóa / vai trò Staff trước khi liên kết.
14. **Khóa do admin đặt (`LockoutEnd = MaxValue`) không bị gỡ khi đặt lại mật khẩu**; chỉ khóa tạm do nhập sai mật khẩu được gỡ.
15. **Xoay vòng refresh token dùng UPDATE có điều kiện** (`WHERE revoked_at IS NULL`): hai lượt refresh song song cùng token thì chỉ một lượt thắng.
16. **`orders.last_changed_at` được cập nhật bởi mọi thay đổi đơn (kể cả chỉ đổi thanh toán)** để concurrency token `xmin` của đơn luôn được kiểm tra; `products` có `xmin` để không mất lượt cộng `SoldCount`.
17. **Hoàn tiền là bước riêng:** hủy đơn đã thanh toán → `Payment.RefundPending`; nhân viên chuyển trả xong mới bấm "đã hoàn tiền" (`POST /admin/orders/{id}/payment/mark-refunded`). Khách không tự hủy được đơn đã thanh toán (`Orders.PaidOrderNeedsStaff`).
18. **Vi phạm unique index khi commit → 409 `Concurrency.DuplicateRequest`** (thay vì 500), ví dụ hai lần checkout cùng Idempotency-Key hoặc hai request đầu tiên cùng tạo giỏ hàng. Idempotency-Key chỉ trả lại đơn cho cùng email người mua (`Orders.IdempotencyKeyReused`).
19. **Job hủy đơn quá hạn xử lý từng đơn trong scope/transaction riêng**; một đơn lỗi được log và bỏ qua, không chặn các đơn khác.
20. **Hàng đợi email không bao giờ chặn request**: đầy (SMTP sập lâu) thì bỏ email mới và log lỗi.
21. **Rate limit tách hai mức:** endpoint nhận thông tin đăng nhập (login, register, google, forgot/reset) 10/phút/IP; endpoint phiên (refresh, logout, confirm-email) 120/phút/IP vì refresh chạy mỗi lần tải trang và nhiều khách dùng chung IP sau CGNAT của nhà mạng. BFF Next.js gửi IP thật của khách kèm khóa bí mật (`RateLimiting:TrustedProxyKey` ↔ `BFF_PROXY_KEY`). Nginx không giới hạn thêm (nó chỉ thấy IP của BFF).
22. **Token bearer sai/hết hạn trả 401 `Auth.InvalidToken` ở mọi endpoint**, kể cả endpoint ẩn danh, để checkout với token hết hạn không âm thầm thành đơn khách vãng lai. Response 401/403/404/429 do framework sinh có mã ổn định (`Auth.NotAuthenticated`, `Auth.Forbidden`, `Http.NotFound`, `Http.TooManyRequests`).
23. **Khóa Data Protection lưu vào volume** (`DataProtection:KeysPath`) để link đặt lại mật khẩu / xác nhận email không hết hiệu lực sau mỗi lần deploy.

## Hệ quả

Tốt: ít thành phần hơn, mọi bất biến quan trọng (kho, idempotency) nằm trong một transaction; deploy tái lập được.

Đánh đổi: email có thể mất khi restart đúng lúc; tìm kiếm `LIKE '%x%'` quét bảng khi catalog rất lớn (chưa phải vấn đề với vài nghìn sản phẩm); phụ thuộc dịch vụ ngoài cho ảnh QR (nội dung chuyển khoản vẫn hiển thị dạng chữ nếu ảnh lỗi).
