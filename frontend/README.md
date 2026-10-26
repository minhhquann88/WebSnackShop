# SnackShop frontend

Storefront + admin cho SnackShop. Next.js 16 (App Router, TypeScript strict), Tailwind CSS 4, các component kiểu shadcn/ui
copy trong `src/components/ui`, TanStack Query, Zustand, React Hook Form + Zod, client API sinh từ `backend/openapi.json`
(`openapi-typescript` + `openapi-fetch`). Quy ước chung: [docs/11-frontend-guidelines.md](../docs/11-frontend-guidelines.md).

## Yêu cầu

- Node.js ≥ 20.9 (đã thử với Node 24), pnpm 10 (`corepack enable`).
- API SnackShop đang chạy (mặc định `http://localhost:5000`, CORS cho `http://localhost:3000`).

## Chạy dev

```bash
cd frontend
cp .env.example .env.local   # chỉnh nếu cần
pnpm install
pnpm dev                     # http://localhost:3000
```

Tài khoản admin seed: `admin@snackshop.local` / `Admin12345`. Email (xác nhận, đặt lại mật khẩu, đơn hàng) xem ở Mailpit
`http://localhost:8025`.

## Biến môi trường

| Biến                           | Bắt buộc | Mô tả                                                                                                                                       |
| ------------------------------ | -------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| `NEXT_PUBLIC_API_BASE_URL`     | có       | Origin của API, không có `/` cuối (`http://localhost:5000`). Được nhúng vào bundle lúc build.                                               |
| `NEXT_PUBLIC_GOOGLE_CLIENT_ID` | không    | Client ID Google Identity Services. Để trống → ẩn nút "Tiếp tục với Google".                                                                |
| `API_INTERNAL_URL`             | không    | URL API chỉ dùng phía server (SSR, BFF `/api/auth/*`), ví dụ `http://api:8080` trong docker compose. Mặc định = `NEXT_PUBLIC_API_BASE_URL`. |
| `AUTH_COOKIE_SECURE`           | không    | `true`/`false` ép cờ `Secure` của cookie refresh token. Mặc định: bật khi `NODE_ENV=production`.                                            |

`NEXT_PUBLIC_*` là public, không đặt secret vào đó.

## Lệnh

| Lệnh                                | Việc                                                                                                                  |
| ----------------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| `pnpm dev`                          | Dev server (Turbopack) cổng 3000                                                                                      |
| `pnpm build` / `pnpm start`         | Build production / chạy bản build (`node .next/standalone/server.js` cho bản standalone)                              |
| `pnpm lint`                         | ESLint (next/core-web-vitals + typescript-eslint strict + prettier)                                                   |
| `pnpm typecheck`                    | `next typegen && tsc --noEmit`                                                                                        |
| `pnpm test`                         | Vitest + Testing Library (format, map lỗi, giỏ hàng, PriceTag)                                                        |
| `pnpm format` / `pnpm format:check` | Prettier (+ sắp xếp class Tailwind)                                                                                   |
| `pnpm gen:api`                      | Sinh lại `src/lib/api/schema.d.ts` từ `../backend/openapi.json` — **không sửa tay file sinh**, commit kèm khi API đổi |

## Cấu trúc

```
src/
  app/
    (storefront)/            header/footer shop; trang công khai + tài khoản
    (admin)/admin/           layout sidebar, kiểm tra role Staff/Admin
    api/auth/[action]/       BFF: login | register | google | refresh | logout
    layout.tsx, providers.tsx, error.tsx, not-found.tsx
  proxy.ts                   (Next 16, trước đây middleware.ts) chặn /admin, /tai-khoan khi chưa có cookie phiên
  components/ui|storefront|admin|common
  features/<module>/         api.ts (gọi client sinh) + queries.ts (TanStack Query) + schemas.ts (Zod)
  lib/api/                   client.ts (trình duyệt, interceptor auth), server.ts (SSR, ISR 60 s), schema.d.ts (sinh)
  lib/auth/                  auth.store.ts (token trong memory), session.ts (refresh single-flight)
  lib/errors.ts, format.ts, labels.ts
  messages/errors.vi.json    mã lỗi API → tiếng Việt
  stores/cart.store.ts       giỏ hàng khách (persist localStorage)
```

## Các route

Storefront: `/`, `/san-pham` (tìm kiếm `?search=&sort=&page=`), `/san-pham/[slug]`, `/danh-muc/[slug]`, `/gio-hang`,
`/thanh-toan`, `/dat-hang-thanh-cong?orderNumber=`, `/tra-cuu-don?orderNumber=`, `/dang-nhap`, `/dang-ky`, `/quen-mat-khau`,
`/dat-lai-mat-khau?email=&token=`, `/xac-nhan-email?userId=&token=`, `/tai-khoan` (hồ sơ), `/tai-khoan/mat-khau`,
`/tai-khoan/dia-chi`, `/tai-khoan/don-hang`, `/tai-khoan/don-hang/[orderNumber]`.

Admin: `/admin` (tổng quan), `/admin/don-hang`, `/admin/don-hang/[id]`, `/admin/san-pham`, `/admin/san-pham/moi`,
`/admin/san-pham/[id]`, `/admin/danh-muc`, `/admin/kho`, `/admin/kho/[variantId]`, `/admin/khach-hang`,
`/admin/khach-hang/[id]`, `/admin/nhan-vien`, `/admin/bao-cao`, `/admin/cai-dat`. Mục chỉ dành cho Admin bị ẩn với Staff.

## Xác thực (BFF)

1. Form đăng nhập gọi `POST /api/auth/login` (Route Handler của Next) → handler gọi API `/auth/login`, lưu refresh token
   vào cookie `ss_refresh` (`httpOnly; SameSite=Lax; Path=/; Secure` ở production) và chỉ trả `accessToken` + `user`.
2. Access token chỉ nằm trong memory (Zustand, không persist). Khi tải trang, `SessionBootstrap` gọi `/api/auth/refresh`
   để khôi phục phiên.
3. Refresh token xoay vòng mỗi lần dùng và dùng lại token cũ sẽ thu hồi mọi phiên, nên refresh luôn **single-flight**:
   một promise dùng chung trong tab, Web Locks API giữa các tab, và BFF nhớ kết quả refresh theo token trong 30 giây để
   hai request trùng cookie nhận cùng cặp token mới.
4. `openapi-fetch` dùng `fetch` có gắn `Authorization`; làm mới token trước khi hết hạn 30 giây; nhận `401` → refresh một
   lần rồi thử lại; refresh thất bại → xoá phiên, chuyển `/dang-nhap?returnUrl=`.
5. Sau khi đăng nhập, giỏ hàng khách (localStorage) được `POST /me/cart/merge` rồi xoá local.
6. `proxy.ts` chỉ kiểm tra có cookie; quyền thật kiểm tra ở layout admin (role) và luôn ở API.

## Docker (tự host, tuỳ chọn)

```bash
docker build -t snackshop-web \
  --build-arg NEXT_PUBLIC_API_BASE_URL=https://api.snackshop.vn \
  --build-arg NEXT_PUBLIC_GOOGLE_CLIENT_ID= .
docker run -p 3000:3000 -e API_INTERNAL_URL=http://api:8080 snackshop-web
```

Image dùng `output: 'standalone'`, chạy bằng user không phải root (`nextjs`, uid 1001).

## Ghi chú

- Ảnh sản phẩm dùng `<img>` thường (`components/ui/remote-image.tsx`) vì ảnh đến từ placehold.co và `/uploads` của API
  (localhost bị `next/image` chặn mặc định ở Next 16). Khi có CDN ảnh cố định, có thể chuyển sang `next/image` +
  `images.remotePatterns`.
- Rate limit `/auth/*` của API tính theo IP; khi đi qua BFF, IP nguồn là server Next. BFF có chuyển tiếp
  `X-Forwarded-For`, nhưng API cần bật ForwardedHeaders (tin proxy) thì giới hạn mới tính theo IP người dùng.
