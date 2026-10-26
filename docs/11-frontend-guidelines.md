# 11 — Frontend guidelines (Next.js)

Frontend sống ở `frontend/`, là **client của API**, không chứa logic nghiệp vụ (tính tiền, kiểm kho, quyền). Mọi tính toán hiển thị (subtotal trong giỏ) là tạm, server tính lại lúc checkout qua `POST /orders/estimate`.

Đã triển khai (Next.js 16). Hướng dẫn chạy/biến môi trường: [frontend/README.md](../frontend/README.md).

## 1. Stack

| Thành phần | Lựa chọn | Ghi chú |
|---|---|---|
| Framework | Next.js 16 (App Router), React 19, TypeScript 5.9 `strict` | Pin version chính xác trong `package.json` (`.npmrc save-exact`) |
| Package manager | `pnpm` | Lockfile commit |
| UI | Tailwind CSS 4 + component kiểu shadcn/ui (Radix) | Component copy vào `src/components/ui`, dễ chỉnh. Biểu đồ admin: Recharts |
| Server state | TanStack Query | Cache, retry, invalidate |
| Client state | Zustand (giỏ hàng guest, UI state) | Nhỏ gọn; persist `localStorage` cho cart |
| Form | React Hook Form + Zod | Schema Zod sinh từ OpenAPI khi có thể |
| API client | `openapi-typescript` (types) + `openapi-fetch` (client) | Sinh từ `backend` `openapi.json`, commit kết quả |
| Auth client | Google Identity Services script | |
| Test | Vitest + Testing Library; Playwright (E2E Phase 3) | |
| Lint/format | ESLint (next/core-web-vitals, typescript-eslint strict), Prettier | |

## 2. Cấu trúc

```
frontend/
  src/
    app/
      (storefront)/           layout có header/footer shop
        page.tsx              trang chủ
        san-pham/[slug]/page.tsx
        danh-muc/[slug]/page.tsx
        gio-hang/page.tsx
        thanh-toan/page.tsx
        dat-hang-thanh-cong/page.tsx  ?orderNumber= (QR chuyển khoản)
        tra-cuu-don/page.tsx  ?orderNumber= (link trong email đơn hàng)
        san-pham/page.tsx     danh sách + tìm kiếm ?search=&sort=&page=
        tai-khoan/            hồ sơ, mat-khau/, dia-chi/, don-hang/, don-hang/[orderNumber]/
        dang-nhap/, dang-ky/, quen-mat-khau/, dat-lai-mat-khau/, xac-nhan-email/
      (admin)/admin/          layout sidebar, bảo vệ bằng role
        page.tsx              dashboard
        san-pham/, danh-muc/, don-hang/, kho/, khach-hang/, nhan-vien/, bao-cao/, cai-dat/
      api/auth/[action]/route.ts BFF: login|register|google|refresh|logout, refresh token trong cookie httpOnly
      layout.tsx, not-found.tsx, error.tsx
    proxy.ts                  (Next 16 đổi tên middleware.ts → proxy.ts) chặn /admin, /tai-khoan khi chưa có cookie
    components/
      ui/                     shadcn
      storefront/             ProductCard, CartDrawer, PriceTag...
      admin/                  DataTable, OrderStatusBadge, StockBadge...
    features/                 hook + query theo module: products/, cart/, orders/, auth/
      orders/
        api.ts                wrapper gọi client sinh
        queries.ts            useOrdersQuery, useOrderQuery, usePlaceOrderMutation
        schemas.ts            Zod cho form checkout
    lib/
      api/
        client.ts             openapi-fetch instance + interceptor auth
        schema.d.ts           SINH TỰ ĐỘNG, không sửa tay
      auth/                   token store, useSession
      format.ts               formatVnd, formatDate (Asia/Ho_Chi_Minh)
      errors.ts               map ProblemDetails.title → message tiếng Việt
    stores/
      cart.store.ts           Zustand, persist
    messages/
      errors.vi.json          "Orders.InvalidStatusTransition": "Đơn hàng không thể chuyển sang trạng thái này."
  e2e/
  public/
```

URL storefront **tiếng Việt không dấu** (`/san-pham/kho-ga-la-chanh`) để SEO và thân thiện. Admin URL cũng tiếng Việt cho nhất quán.

## 3. Gọi API

- **Chỉ** qua client sinh từ OpenAPI. Không tự viết `fetch('/api/v1/...')` với URL chuỗi.
- Regenerate: `pnpm gen:api` → đọc `../backend/openapi.json` (hoặc từ API đang chạy) → `src/lib/api/schema.d.ts`. CI kiểm diff.
- Server Components (SSR) gọi API trực tiếp bằng `fetch` của Next với `next: { revalidate: 60 }` cho catalog public (ISR). Client Components dùng TanStack Query.
- Error handling tập trung: `ProblemDetails` → `ApiError { status, code: title, errors }`. UI hiển thị `messages/errors.vi.json[code]` hoặc message chung. `errors` (400) map vào field form bằng React Hook Form `setError`.
- Hiển thị tiền: `formatVnd(45000) => "45.000 ₫"`. Không bao giờ tự tính tổng cuối, dùng `/orders/estimate`.
- Ngày giờ: nhận ISO UTC, hiển thị bằng `Intl.DateTimeFormat('vi-VN', { timeZone: 'Asia/Ho_Chi_Minh' })`.

## 4. Auth phía client

- **Access token**: giữ trong memory (Zustand, không persist). Hết hạn 15 phút.
- **Refresh token**: cookie `httpOnly; Secure; SameSite=Lax` do Route Handler `/api/auth/*` của Next set. Browser JS không đọc được → giảm rủi ro XSS.
- Luồng: login → gọi Route Handler → Route Handler gọi backend `/auth/login` → set cookie refresh, trả access token về client.
- Interceptor của `openapi-fetch`: nhận `401` → gọi `/api/auth/refresh` một lần → retry request; fail → logout, chuyển `/dang-nhap?returnUrl=`.
- `src/proxy.ts` (Next 16 thay `middleware.ts`) chặn `/admin` và `/tai-khoan` nếu không có cookie; role kiểm tra lại ở layout (theo `user.roles` của phiên) và **server luôn kiểm tra lại**.
- Refresh là **single-flight** (refresh token xoay vòng, dùng lại token cũ sẽ thu hồi mọi phiên): một promise dùng chung trong tab, Web Locks giữa các tab, BFF nhớ kết quả refresh theo token 30 giây.
- Guest cart: Zustand persist `localStorage`; sau login gọi `POST /me/cart/merge`, rồi xóa local.

## 5. Quy tắc code

- TypeScript `strict`, không `any` (dùng `unknown` + narrow). ESLint `no-explicit-any: error`.
- Component: function component, `PascalCase`, một component chính mỗi file, props type đặt ngay trên component.
- Server Component mặc định; thêm `'use client'` chỉ khi cần state/effect/event.
- Không gọi API trong `useEffect` tay; dùng TanStack Query.
- Không business rule trong UI: ví dụ nút "Hủy đơn" hiện khi `order.canCancel` (field do API trả), **không** tự suy `status === 'Pending'` ở nhiều chỗ. Backend thêm các field `can*` vào response để UI dumb.
- Tailwind class order theo plugin `prettier-plugin-tailwindcss`. Không CSS module trừ animation phức tạp.
- Ảnh sản phẩm hiện dùng `<img>` qua `components/ui/remote-image.tsx` (ảnh đến từ placehold.co và `/uploads` của API; Next 16 chặn host localhost/IP nội bộ trong `next/image`). Khi có CDN ảnh cố định → chuyển sang `next/image` + `images.remotePatterns`.
- Accessibility: mọi input có label, nút icon có `aria-label`, contrast đạt AA. shadcn đã lo phần lớn.
- i18n: chưa cần thư viện; chuỗi tiếng Việt viết thẳng trong component nhưng **mã lỗi** phải qua `errors.vi.json`.

## 6. Môi trường

```
NEXT_PUBLIC_API_BASE_URL=https://api.snackshop.vn
NEXT_PUBLIC_GOOGLE_CLIENT_ID=...
API_INTERNAL_URL=http://api:8080          # chỉ server-side nếu self-host cùng compose
AUTH_COOKIE_SECURE=                       # tuỳ chọn, mặc định Secure khi production
```

`NEXT_PUBLIC_*` là public, không đặt secret.

## 7. Deploy

Ưu tiên **Vercel** (free, CDN, preview deploy mỗi PR). Env đặt trên Vercel. Nếu tự host: Dockerfile `output: 'standalone'`, thêm service `web` vào `infra/docker-compose.prod.yml`, Nginx route `snackshop.vn` → `web:3000`.
