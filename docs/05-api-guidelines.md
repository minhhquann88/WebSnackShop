# 05 — API guidelines

API là **hợp đồng** giữa backend và mọi client. Thay đổi phá vỡ (breaking) phải tăng version. OpenAPI sinh tự động từ code là tài liệu chính thức; file này quy định cách thiết kế.

## 1. URL và tài nguyên

- Prefix: `/api/v1`. Admin: `/api/v1/admin`. Tài khoản của chính user: `/api/v1/me`.
- Danh từ **số nhiều**, `kebab-case`: `/products`, `/order-items` (không `/getProducts`, `/product`).
- Định danh: dùng `id` (Guid) cho admin; dùng `slug` cho public catalog; dùng `orderNumber` cho đơn hàng public.
- Hành động không CRUD: `POST /orders/{id}/cancel`, `POST /admin/orders/{id}/confirm`. Không dùng `PATCH { status: "..." }` để đổi trạng thái vì mỗi chuyển trạng thái có rule riêng.
- Lồng tối đa 2 cấp: `/admin/products/{id}/variants`. Sâu hơn → tài nguyên riêng.
- Query string cho lọc/sắp xếp/phân trang: `?categorySlug=banh&search=kho%20ga&sort=-createdAt&page=1&pageSize=20`.
  - `sort`: tên field camelCase, tiền tố `-` là giảm dần. Chỉ cho phép field trong whitelist của validator.
- Không có trailing slash. Không có verb trong URL.

## 2. HTTP method và status code

| Hành động | Method | Thành công | Ghi chú |
|---|---|---|---|
| Lấy danh sách | `GET` | `200` + `PagedResponse<T>` | Luôn phân trang |
| Lấy một | `GET` | `200` | `404` nếu không có |
| Tạo | `POST` | `201` + `Location` header + body response | |
| Cập nhật toàn phần | `PUT` | `200` + body hoặc `204` | |
| Cập nhật một phần | `PATCH` | `200` | Hạn chế dùng, chỉ cho field độc lập (ví dụ `isActive`) |
| Xóa | `DELETE` | `204` | Soft delete với catalog; `404` nếu không có |
| Hành động nghiệp vụ | `POST /{id}/{action}` | `200` + trạng thái mới | `409` nếu sai trạng thái |

Map từ `Error.Type` (xem [04-coding-conventions.md](04-coding-conventions.md#4-result-pattern-và-xử-lý-lỗi)):

| `ErrorType` | HTTP |
|---|---|
| `Validation` | `400` |
| `Unauthorized` | `401` |
| `Forbidden` | `403` |
| `NotFound` | `404` |
| `Conflict` | `409` |
| `Failure` | `500` |

Thêm: `429` cho rate limit, `415`/`413` cho upload sai định dạng/quá lớn.

## 3. Định dạng JSON

- `camelCase` property. `System.Text.Json`, không Newtonsoft.
- Enum serialize dạng **string** (`"status": "Pending"`), `JsonStringEnumConverter` global.
- Ngày giờ: ISO 8601 có offset, luôn UTC từ server: `"2026-10-22T03:15:00+00:00"`. Client tự convert sang giờ Việt Nam.
- Tiền: object `{ "amount": 45000, "currency": "VND" }`. Không trả số trần.
- Guid: string chuẩn lowercase.
- `null` field vẫn trả (không bỏ qua) để client biết field tồn tại. Ngoại lệ: không trả field nội bộ (`InternalNote` ở public API).
- Không bao gói `{ "data": ..., "success": true }`. Body thành công là object thẳng; lỗi là ProblemDetails (mục 4). Status code đã đủ để biết thành công hay thất bại.

## 4. Lỗi: RFC 9457 ProblemDetails

Mọi lỗi (4xx, 5xx) trả `Content-Type: application/problem+json`:

```json
{
  "type": "https://snackshop.vn/errors/orders/invalid-status-transition",
  "title": "Orders.InvalidStatusTransition",
  "status": 409,
  "detail": "The order cannot move to the requested status.",
  "instance": "/api/v1/admin/orders/0192f.../confirm",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

Lỗi validation (400) thêm `errors`:

```json
{
  "type": "https://snackshop.vn/errors/validation",
  "title": "Validation.Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "items[0].quantity": ["Quantity must be between 1 and 99."],
    "shippingAddress.phone": ["Phone must be 10 digits starting with 0."]
  },
  "traceId": "..."
}
```

- `title` = `Error.Code` (ổn định, frontend map sang tiếng Việt). `detail` = message tiếng Anh cho dev.
- `traceId` luôn có để tra log.
- 500 **không** trả stack trace hay message exception ở Production. Chỉ `"detail": "An unexpected error occurred."`.
- Frontend không hiển thị `detail` trực tiếp cho người dùng cuối; map theo `title`.

## 5. Phân trang, lọc, sắp xếp

Request: `?page=1&pageSize=20` (mặc định `page=1`, `pageSize=20`, tối đa `100`).

Response:

```json
{
  "items": [ ... ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 137,
  "totalPages": 7,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

Record dùng chung: `PagedResponse<T>` trong `Application/Common`. Offset pagination đủ cho MVP; cursor pagination chỉ khi có bảng > 1 triệu dòng.

## 6. Versioning

- Version trong URL (`/api/v1`). Dễ hiểu, dễ cache, dễ route ở Nginx.
- Thay đổi **không phá vỡ** (thêm field, thêm endpoint, thêm giá trị enum có default) → giữ v1.
- Thay đổi **phá vỡ** (xóa/đổi tên field, đổi kiểu, đổi semantic) → `/api/v2` cho endpoint đó, giữ v1 ít nhất 1 release.
- Frontend trong cùng monorepo nên thực tế hiếm khi cần v2; nhưng khi có mobile app thì quy tắc này bắt buộc.

## 7. Xác thực và phân quyền ở API

- Header `Authorization: Bearer <access_token>`. Refresh token gửi qua body `POST /auth/refresh` (xem [07-auth-security.md](07-auth-security.md)).
- Policy đặt tên trong `Policies`:
  - `Policies.Customer` – role `Customer` trở lên (bất kỳ user đăng nhập).
  - `Policies.Staff` – role `Staff` hoặc `Admin`.
  - `Policies.Admin` – role `Admin`.
- Endpoint `/me/*` yêu cầu đăng nhập và **chỉ** trả dữ liệu của `currentUser.UserId`. Không nhận `customerId` từ client.
- Endpoint public (`/products`, `/categories`, `POST /orders`, `GET /orders/track`) `AllowAnonymous` tường minh.
- Admin endpoint luôn ở `/admin/*` với `Policies.Staff` tối thiểu, để Nginx có thể chặn theo IP nếu cần.

## 8. Idempotency và an toàn

- `POST /orders` nhận header tùy chọn `Idempotency-Key` (chuỗi `[A-Za-z0-9_-]{1,64}`, client sinh một lần cho mỗi lần checkout). Cùng key → trả lại đơn cũ (201), không tạo đơn đôi khi mạng chập chờn. Key lưu ở cột unique `orders.idempotency_key` (ADR-0009).
- Body JSON sai cú pháp/kiểu → `400` với `title = "Request.Malformed"` ở mọi môi trường.
- Mọi `POST/PUT/PATCH/DELETE` yêu cầu `Content-Type: application/json` (trừ upload `multipart/form-data`).
- Rate limit (cấu hình `RateLimiting:*`): `/auth/login|register|google|forgot-password|reset-password` và `GET /orders/track`: 10 req/phút/IP. `/auth/refresh|logout|confirm-email`: 120 req/phút/IP. `POST /orders`: 5 req/phút/IP. Trả `429` + `Retry-After`, `title = "Http.TooManyRequests"`. Request đi qua BFF tin cậy được tính theo IP thật của khách (ADR-0009 §21).
- Token bearer sai hoặc hết hạn → `401 Auth.InvalidToken` ở mọi endpoint (kể cả ẩn danh). Không có token khi cần đăng nhập → `401 Auth.NotAuthenticated`; sai quyền → `403 Auth.Forbidden`.
- `POST /admin/orders/{id}/payment/mark-refunded` `{ transactionRef }`: xác nhận đã hoàn tiền cho đơn đã hủy có `payment.status = RefundPending` (`actions.canMarkRefunded`).
- CORS: chỉ origin của frontend (cấu hình `Cors:AllowedOrigins`). Không `AllowAnyOrigin` ở Production.
- Upload ảnh: `multipart/form-data`, tối đa 5 MB, chỉ `image/jpeg|png|webp`, kiểm tra magic bytes, đổi tên file thành Guid, trả URL.

## 9. OpenAPI

- Dùng `Microsoft.AspNetCore.OpenApi` + Scalar UI tại `/scalar` (chỉ Development và Staging).
- Mọi endpoint phải có: `.WithName("PlaceOrder")` (= operationId, frontend client dùng tên này), `.WithSummary()`, `.WithTags("Orders")`, `.Produces<T>(201)`, `.ProducesProblem(400)`, `.ProducesProblem(409)`.
- File `openapi.json` export trong CI (`dotnet run -- --export-openapi`), frontend sinh client bằng `openapi-typescript` + `openapi-fetch`. Diff `openapi.json` trong PR là cách review hợp đồng API.

## 10. Danh sách endpoint MVP

### Public / Storefront

| Method | Route | Mô tả | Auth |
|---|---|---|---|
| GET | `/categories` | Danh mục active, sắp theo `sortOrder` | Anon |
| GET | `/products` | List + filter `categorySlug, search, sort, page, pageSize, featured` | Anon |
| GET | `/products/{slug}` | Chi tiết + variants + images + tồn kho (chỉ `available > 0` dạng bool/số) | Anon |
| GET | `/settings/public` | Phí ship, ngưỡng miễn ship, thông tin shop | Anon |
| POST | `/orders` | Đặt hàng (guest hoặc customer). Body có `items[]`, `shippingAddress`, `contactEmail`, `paymentMethod`, `customerNote` | Anon (gắn `customerId` nếu có token) |
| GET | `/orders/track?orderNumber=&phone=` | Tra cứu đơn guest | Anon |
| POST | `/orders/estimate` | Tính subtotal/ship/total từ giỏ (dùng ở checkout) | Anon |

### Auth

| Method | Route | Mô tả |
|---|---|---|
| POST | `/auth/register` | Email, password, fullName, phone |
| POST | `/auth/login` | → `accessToken`, `refreshToken`, `expiresIn`, `user` |
| POST | `/auth/google` | `idToken` từ Google → cùng response login |
| POST | `/auth/refresh` | `refreshToken` → cặp token mới (rotation) |
| POST | `/auth/logout` | Revoke refresh token |
| POST | `/auth/forgot-password` | Gửi mail link reset |
| POST | `/auth/reset-password` | `token`, `newPassword` |
| POST | `/auth/confirm-email` | `userId`, `token` |

### Me (Customer)

| Method | Route | Mô tả |
|---|---|---|
| GET / PUT | `/me` | Hồ sơ |
| PUT | `/me/password` | Đổi mật khẩu |
| GET / POST | `/me/addresses` | Sổ địa chỉ |
| PUT / DELETE | `/me/addresses/{id}` | |
| POST | `/me/addresses/{id}/default` | |
| GET | `/me/cart` | Giỏ + giá hiện tại + tồn |
| POST | `/me/cart/merge` | Gộp giỏ khách vãng lai sau khi đăng nhập (cộng dồn, bỏ món không còn bán) — ADR-0009 |
| POST | `/me/cart/items` | `variantId, quantity` (cộng dồn) |
| PUT / DELETE | `/me/cart/items/{variantId}` | Sửa số lượng / xóa |
| DELETE | `/me/cart` | Xóa giỏ |
| GET | `/me/orders` | Lịch sử đơn, phân trang |
| GET | `/me/orders/{orderNumber}` | Chi tiết |
| POST | `/me/orders/{orderNumber}/cancel` | Chỉ khi `Pending` |

### Admin (Staff/Admin)

| Method | Route | Mô tả | Policy |
|---|---|---|---|
| GET | `/admin/reports/dashboard` | Doanh thu hôm nay/7 ngày/30 ngày, đơn theo trạng thái, sắp hết hàng | Staff |
| GET | `/admin/reports/revenue?from=&to=&groupBy=day` | Series doanh thu | Admin |
| GET | `/admin/reports/top-products?from=&to=&limit=10` | | Admin |
| GET/POST | `/admin/categories`, PUT/DELETE `/admin/categories/{id}`, PUT `/admin/categories/order` `{ids}` | | Admin |
| GET/POST | `/admin/products`, GET/PUT/DELETE `/admin/products/{id}` | List có cả inactive, filter | Admin |
| POST | `/admin/products/{id}/variants`, PUT/DELETE `/admin/products/{id}/variants/{variantId}` | | Admin |
| POST | `/admin/products/{id}/images` (multipart), DELETE `/admin/products/{id}/images/{imageId}`, POST `.../images/{imageId}/primary`, PUT `.../images/order` | | Admin |
| PATCH | `/admin/products/{id}/status` | `isActive`, `isFeatured` | Admin |
| GET | `/admin/inventory` | Tồn theo variant, filter `lowStock=true`, search | Staff |
| GET | `/admin/inventory/{variantId}/movements` | Lịch sử | Staff |
| POST | `/admin/inventory/{variantId}/receive` | Nhập kho `quantity, note` | Staff |
| POST | `/admin/inventory/{variantId}/adjust` | `delta, reason` | Admin |
| PUT | `/admin/inventory/{variantId}/threshold` | | Admin |
| GET | `/admin/orders` | Filter `status, from, to, search(orderNumber/phone/email), paymentMethod, paymentStatus` | Staff |
| GET | `/admin/orders/{id}` | Chi tiết đầy đủ + history + payment | Staff |
| POST | `/admin/orders/{id}/confirm` | Body tùy chọn `{ markPaid: true, transactionRef }` cho CK | Staff |
| POST | `/admin/orders/{id}/prepare` | | Staff |
| POST | `/admin/orders/{id}/ship` | `{ note }` | Staff |
| POST | `/admin/orders/{id}/deliver` | | Staff |
| POST | `/admin/orders/{id}/cancel` | `{ reason }` | Staff |
| POST | `/admin/orders/{id}/payment/mark-paid` | `{ transactionRef, paidAt }` | Staff |
| PUT | `/admin/orders/{id}/internal-note` | | Staff |
| GET | `/admin/customers`, GET `/admin/customers/{id}` | | Staff |
| GET / PUT | `/admin/settings` | ShopSettings | Admin |
| GET / POST | `/admin/users`, PUT `/admin/users/{id}/roles`, POST `/admin/users/{id}/lock` | Quản lý Staff | Admin |

### Hệ thống

| Route | Mô tả |
|---|---|
| `GET /health` | Liveness (200 nếu process sống) |
| `GET /health/ready` | Readiness (DB ping) |
| `GET /scalar`, `GET /openapi/v1.json` | Chỉ non-production |

## 11. Ví dụ endpoint chuẩn

```csharp
public sealed class OrdersEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRoutes.V1 + "/orders").WithTags("Orders");

        group.MapPost("/", PlaceOrder)
            .WithName("PlaceOrder")
            .WithSummary("Place a new order (guest or authenticated customer).")
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.PlaceOrder)
            .Produces<PlaceOrderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> PlaceOrder(
        PlaceOrderRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(request.ToCommand(), ct);
        return result.Match(
            onSuccess: r => Results.Created($"/api/v1/orders/track?orderNumber={r.OrderNumber}", r),
            onFailure: ResultExtensions.ToProblem);
    }
}
```
