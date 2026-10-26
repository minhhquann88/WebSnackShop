# 01 — Tổng quan dự án

## 1. Mục tiêu

Xây dựng hệ thống bán đồ ăn vặt trực tuyến cho **một shop (hoặc chuỗi nhỏ)**, gồm:

1. **Storefront** — web cho khách hàng: xem danh mục, tìm sản phẩm, thêm vào giỏ, đặt hàng, theo dõi đơn.
2. **Admin** — trang quản trị cho chủ shop và nhân viên: quản lý sản phẩm, kho, đơn hàng, khách hàng, báo cáo doanh thu.
3. **Backend API** — một dịch vụ C#/.NET duy nhất phục vụ cả hai, thiết kế để sau này cắm thêm mobile app, cổng thanh toán, đơn vị vận chuyển mà không đập đi xây lại.

Mục tiêu kỹ thuật song song: codebase **dễ đọc, dễ test, dễ bàn giao**. Người mới đọc `docs/` xong phải viết được feature đúng chuẩn mà không cần hỏi.

## 2. Người dùng và vai trò

| Vai trò | Mô tả | Quyền chính |
|---|---|---|
| **Guest** | Khách chưa đăng nhập | Xem sản phẩm, giỏ hàng (lưu phía client), đặt hàng không cần tài khoản, tra cứu đơn bằng mã đơn + SĐT |
| **Customer** | Khách đã đăng ký (email/mật khẩu hoặc Google) | Tất cả của Guest + sổ địa chỉ, lịch sử đơn, giỏ hàng đồng bộ server, hủy đơn khi còn `Pending` |
| **Staff** | Nhân viên shop | Xem/xử lý đơn hàng, cập nhật trạng thái, nhập kho, xem sản phẩm |
| **Admin** | Chủ shop | Tất cả của Staff + CRUD sản phẩm/danh mục, cấu hình phí ship, quản lý nhân viên, xem báo cáo |

## 3. Phạm vi MVP (phải có để ra mắt)

### Storefront
- [ ] Trang chủ: sản phẩm nổi bật, danh mục.
- [ ] Danh sách sản phẩm theo danh mục, tìm kiếm theo tên, sắp xếp (mới nhất / giá / bán chạy), phân trang.
- [ ] Chi tiết sản phẩm: ảnh, mô tả, thành phần, **biến thể** (vị / khối lượng), giá, tồn kho còn/hết.
- [ ] Giỏ hàng: thêm / sửa số lượng / xóa; guest lưu localStorage, đăng nhập thì merge lên server.
- [ ] Checkout: thông tin người nhận, địa chỉ (tỉnh / huyện / xã), ghi chú, chọn **COD** hoặc **chuyển khoản (VietQR)**, phí ship cố định hoặc miễn phí trên ngưỡng.
- [ ] Xác nhận đơn: email xác nhận; nếu chuyển khoản thì hiện mã QR + nội dung chuyển khoản.
- [ ] Tài khoản: đăng ký / đăng nhập (email + Google), quên mật khẩu, hồ sơ, sổ địa chỉ, lịch sử đơn, hủy đơn.
- [ ] Tra cứu đơn hàng cho guest.

### Admin
- [ ] Đăng nhập, phân quyền Staff / Admin.
- [ ] Dashboard: doanh thu hôm nay / tuần / tháng, số đơn theo trạng thái, sản phẩm sắp hết hàng.
- [ ] Danh mục: CRUD, sắp thứ tự, ẩn / hiện.
- [ ] Sản phẩm: CRUD, nhiều ảnh, nhiều biến thể, giá / giá gốc, SKU, ẩn / hiện, nổi bật.
- [ ] Kho: xem tồn theo biến thể, nhập kho, điều chỉnh, lịch sử biến động.
- [ ] Đơn hàng: danh sách + lọc (trạng thái, ngày, SĐT, mã đơn), chi tiết, chuyển trạng thái theo đúng state machine, xác nhận đã nhận tiền chuyển khoản, hủy đơn có lý do.
- [ ] Khách hàng: danh sách, chi tiết, lịch sử mua.
- [ ] Cấu hình: phí ship, ngưỡng miễn ship, thông tin tài khoản ngân hàng nhận tiền.

### Nền tảng kỹ thuật
- [ ] OpenAPI đầy đủ, client TypeScript sinh tự động.
- [ ] Docker Compose chạy toàn bộ local bằng một lệnh.
- [ ] CI: build, test, lint trên mọi PR. CD: deploy VPS khi merge `main`.
- [ ] Health check, structured logging, backup DB hằng ngày.

## 4. Ngoài phạm vi MVP (làm sau, nhưng kiến trúc phải không chặn)

| Tính năng | Dự kiến | Ghi chú thiết kế hiện tại |
|---|---|---|
| Cổng thanh toán VNPay / MoMo / ZaloPay | Phase 4 | `IPaymentGateway` + bảng `payments` sẵn từ MVP |
| Mã giảm giá / khuyến mãi | Phase 4 | `Order` đã có trường `DiscountAmount`, `CouponCode` |
| Đánh giá sản phẩm | Phase 4 | — |
| Tích hợp vận chuyển GHN / GHTK | Phase 4 | `IShippingCalculator` abstraction |
| Thông báo Zalo OA / SMS | Phase 4 | `INotificationSender` |
| Redis cache, Outbox pattern | Khi có tải | `HybridCache` dùng in-memory trước |
| Marketplace nhiều shop, ví, hoa hồng | Không có kế hoạch | — |
| Mobile app | Chưa | API đã stateless, JWT sẵn |

## 5. Ràng buộc và giả định

- Team **1–3 người**, ưu tiên tốc độ học và tốc độ bàn giao hơn tối ưu hiệu năng cực hạn.
- Ngôn ngữ giao diện: **tiếng Việt**. Code, tên biến, commit, tài liệu kỹ thuật: **tiếng Anh** (tài liệu `docs/` viết tiếng Việt để team đọc nhanh, nhưng mọi định danh trong code là tiếng Anh).
- Tiền tệ: **VND**, không có phần thập phân khi hiển thị, nhưng lưu `numeric(18,2)` để không phải migrate khi thêm tiền tệ khác.
- Múi giờ: lưu **UTC** trong DB (`timestamptz`), hiển thị theo `Asia/Ho_Chi_Minh` ở frontend.
- Chạy được trên một VPS 2 vCPU / 4 GB RAM ở giai đoạn đầu.

## 6. Chỉ số thành công của MVP

- Khách đặt được đơn hoàn chỉnh trong **dưới 2 phút** từ trang chủ.
- Admin xử lý một đơn (xác nhận → giao) trong **dưới 5 click**.
- API p95 **< 300 ms** cho các endpoint đọc với 10k sản phẩm.
- `dotnet test` xanh, coverage Domain + Application **≥ 80 %**.
- Người mới clone repo, chạy `docker compose up` + `dotnet run` là có hệ thống chạy trong **< 15 phút**.

## 7. Tài liệu liên quan

- Kiến trúc: [02-architecture.md](02-architecture.md)
- Domain: [03-domain-model.md](03-domain-model.md)
- Kế hoạch: [12-roadmap.md](12-roadmap.md)