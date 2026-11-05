# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

- **Khách mua đồ ăn vặt** (Guest/Customer): người trẻ ở Việt Nam, lướt trên điện thoại lúc rảnh (giờ nghỉ trưa, tối), muốn chọn nhanh vài món quen (khô gà, bánh tráng, hạt sấy, kẹo bánh) rồi đặt giao tận nhà. Đặt hàng không cần tài khoản, thanh toán COD hoặc chuyển khoản VietQR, tra cứu đơn bằng mã đơn + SĐT.
- **Chủ shop và nhân viên** (Admin/Staff): xử lý đơn hàng, nhập kho, quản lý sản phẩm/danh mục, xem báo cáo. Dùng trên laptop, nhiều lần trong ngày, cần quét nhanh và thao tác ít click.

## Product Purpose

Hệ thống bán đồ ăn vặt trực tuyến cho một shop (hoặc chuỗi nhỏ): storefront cho khách và trang quản trị cho shop trên cùng một API. Thành công: khách đặt được đơn hoàn chỉnh trong dưới 2 phút từ trang chủ; admin xử lý một đơn (xác nhận → giao) trong dưới 5 click.

## Positioning

Shop nhỏ bán đồ ăn vặt tuyển chọn, tự đóng gói, giao toàn quốc; không phải marketplace. Mua nhanh không cần tài khoản, thanh toán theo thói quen Việt Nam (COD, chuyển khoản QR).

## Operating Context

- Storefront: trang chủ, danh mục, tìm kiếm, chi tiết sản phẩm có biến thể (vị / khối lượng), giỏ hàng (guest lưu local, đăng nhập thì merge), checkout một trang (người nhận, địa chỉ tỉnh/huyện/xã, ghi chú, COD hoặc chuyển khoản), trang đặt hàng thành công hiện mã VietQR, tra cứu đơn, tài khoản (hồ sơ, đơn hàng, sổ địa chỉ, đổi mật khẩu).
- Admin: tổng quan (doanh thu, đơn theo trạng thái, sắp hết hàng), đơn hàng (lọc, chi tiết, chuyển trạng thái theo state machine, xác nhận chuyển khoản, hủy có lý do), sản phẩm, danh mục, kho (tồn theo biến thể, nhập, điều chỉnh, lịch sử), khách hàng, nhân viên, báo cáo, cài đặt (phí ship, ngưỡng miễn ship, tài khoản ngân hàng).
- Ngôn ngữ giao diện: tiếng Việt. Tiền tệ VND không thập phân. Giờ hiển thị Asia/Ho_Chi_Minh.

## Capabilities and Constraints

- Frontend Next.js 16 App Router, React 19, Tailwind CSS v4, Radix UI (radix-ui), lucide-react, sonner, recharts. Font hiện tại: Be Vietnam Pro qua next/font.
- Ảnh sản phẩm là URL tuyệt đối từ API (placeholder placehold.co ở dev); không có bộ ảnh thương hiệu.
- Trạng thái đơn: Chờ xác nhận → Đã xác nhận → Đang chuẩn bị → Đang giao → Đã giao; Đã hủy. Trạng thái thanh toán: Chưa thanh toán / Đã thanh toán / Hoàn tiền / Thất bại / Chờ hoàn tiền.
- Chưa có: mã giảm giá, đánh giá sản phẩm, cổng thanh toán VNPay/MoMo (Phase 4). Không được vẽ UI cho các tính năng này như đã có.
- Dự án là demo/đồ án (xác nhận 2026-11-04): không có logo, màu thương hiệu hay copy bắt buộc. Tên "SnackShop" giữ nguyên. Người dùng cho phép đổi copy, bố cục, font miễn là chức năng giữ nguyên.

## Brand Commitments

- Tên: SnackShop.
- Ràng buộc "tông màu sáng" pin ngày 2026-11-04 đã được người dùng gỡ ngày 2026-11-05: tông màu mở, tùy hướng thiết kế.
- Người dùng từ chối thế giới hình ảnh "tập ô ly" (2026-11-05): nền kẻ ô, lề đỏ, con dấu nghiêng, danh sách đánh số thay cho lưới sản phẩm, cảm giác nhạt và "không ra shop đồ ăn". Coi là anti-reference cho mọi hướng sau này.

## Evidence on Hand

- Dữ liệu mẫu dev: 5 danh mục, 14 sản phẩm (seed backend, ảnh placehold.co).
- Không có ảnh chụp sản phẩm thật, không có testimonial, không có số liệu khách hàng. Không được bịa các thứ này.

## Product Principles

1. Mua nhanh hơn ngắm: mọi trang storefront dẫn tới giỏ và checkout trong ít bước nhất.
2. Giá và tình trạng còn hàng luôn rõ trước khi khách bấm mua.
3. Admin ưu tiên quét nhanh và trạng thái rõ ràng; hành động nguy hiểm (hủy đơn) luôn có xác nhận.
4. Mọi trạng thái (trống, lỗi, đang tải) đều có giao diện; không có màn hình trắng.
5. Tiếng Việt tự nhiên, không dịch máy.

## Accessibility & Inclusion

- Dùng trên điện thoại là chính với storefront; chạm được, chữ đọc được dưới nắng (tương phản ≥ 4.5:1).
- Giữ điều hướng bàn phím, focus ring, aria hiện có.
