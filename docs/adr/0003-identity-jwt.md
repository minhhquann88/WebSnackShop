# ADR-0003: ASP.NET Core Identity + JWT + refresh token rotation + Google login

- **Trạng thái**: Accepted
- **Ngày**: 2026-10-22
- **Người quyết định**: Chủ dự án

## Bối cảnh

Frontend là SPA tách rời (Next.js), sau này có thể có mobile app. Cần xác thực stateless cho API, có đăng nhập Google cho khách, phân quyền Customer/Staff/Admin. Ứng viên: Identity + JWT tự quản; Identity + cookie; dịch vụ ngoài (Keycloak, Auth0, Firebase Auth).

## Quyết định

Dùng ASP.NET Core Identity lưu user/role trong cùng PostgreSQL. API cấp JWT access token 15 phút (HS256) và refresh token 14 ngày lưu hash trong DB, xoay vòng mỗi lần refresh, phát hiện reuse. Google login qua verify `id_token` ở server. Frontend giữ refresh token trong cookie httpOnly do BFF route của Next.js quản lý.

## Lựa chọn đã cân nhắc

- **Cookie auth của Identity**: đơn giản với MVC, nhưng SPA khác origin và mobile app dùng khó, CSRF phải xử lý.
- **Keycloak**: mạnh, chuẩn OIDC, nhưng thêm một container ~1 GB RAM trên VPS 4 GB, thêm thứ phải vận hành, quá tay cho 1 shop.
- **Auth0 / Firebase Auth**: ít code, nhưng phụ thuộc bên ngoài, giới hạn free tier, dữ liệu user nằm ngoài DB, khó join báo cáo; đổi nhà cung cấp sau này đau.
- **Identity + JWT tự quản**: mọi thứ trong một DB, không chi phí, Identity lo hash/lockout/reset token; phải tự viết refresh token (~200 dòng) và cẩn thận bảo mật.

## Hệ quả

Tốt: không phụ thuộc ngoài, stateless API, cùng mô hình cho web và mobile, dữ liệu user join được với đơn hàng.

Đánh đổi: tự chịu trách nhiệm rotation/reuse detection/secret; revoke access token tức thì không làm được (chấp nhận TTL 15 phút); HS256 phải đổi sang RS256 nếu có service thứ hai verify token.

Việc phải làm theo: test tự động cho rotation và reuse; secret từ env; checklist bảo mật ở docs/07 trước go-live.
