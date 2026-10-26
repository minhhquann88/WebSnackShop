## Mục đích
<!-- 1-3 câu: thay đổi gì, vì sao. Link issue (Closes #...). -->

## Thay đổi chính
-

## Checklist
- [ ] Test thêm/sửa; `dotnet test` (và `pnpm test` nếu chạm frontend) xanh local
- [ ] `dotnet build` 0 warning, `dotnet format --verify-no-changes` pass
- [ ] Migration (nếu có): đã review file `.cs`, tương thích ngược, không sửa migration cũ
- [ ] Đổi API: đã export `backend/openapi.json` và chạy `pnpm gen:api`
- [ ] `docs/` cập nhật nếu đổi nghiệp vụ/kiến trúc; ADR nếu đổi quyết định
- [ ] Không log / commit dữ liệu nhạy cảm (secret, token, mật khẩu, SĐT đầy đủ)

## Cách kiểm tra
<!-- Bước thủ công nếu có (Scalar / UI) -->

## Screenshot (UI)
