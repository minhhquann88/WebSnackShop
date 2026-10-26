# 04 — Quy tắc viết code C#

File `.editorconfig` ở root là **luật được máy kiểm tra** (format, naming, analyzer). Tài liệu này giải thích các quy tắc không thể ép bằng máy. Khi hai bên mâu thuẫn, `.editorconfig` thắng và phải sửa tài liệu.

Build phải **0 warning** trên CI (`TreatWarningsAsErrors=true` trong `Directory.Build.props`).

## 1. Ngôn ngữ, phiên bản, cài đặt chung

- .NET 10, C# 14, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`.
- `Directory.Build.props` ở `backend/` áp cho mọi project: `TreatWarningsAsErrors`, `AnalysisLevel=latest-recommended`, `EnforceCodeStyleInBuild=true`.
- `Directory.Packages.props`: **Central Package Management**, mọi version NuGet khai báo một chỗ.
- File-scoped namespace. Namespace = đường dẫn thư mục (`SnackShop.Application.Features.Orders.PlaceOrder`).
- Một type public mỗi file, tên file = tên type. Ngoại lệ: `record` request/response nhỏ đi kèm command có thể ở cùng file với command đó.

## 2. Naming

| Thứ | Quy tắc | Ví dụ |
|---|---|---|
| Class, record, struct, enum, interface member | `PascalCase` | `OrderItem`, `PlaceOrderCommand` |
| Interface | `I` + PascalCase | `IOrderRepository` |
| Method | PascalCase, động từ | `Confirm()`, `GetByIdAsync()` |
| Async method | Hậu tố `Async` | `SaveChangesAsync` |
| Property | PascalCase, danh từ | `TotalAmount` |
| Private field | `_camelCase` | `_dbContext` |
| Local, parameter | `camelCase` | `orderId` |
| Constant | PascalCase | `MaxImagesPerProduct` |
| Generic type param | `T` + PascalCase | `TResponse` |
| Boolean | Dạng câu hỏi | `IsActive`, `HasVariants`, `CanCancel` |
| Enum | Số ít, member PascalCase | `OrderStatus.Pending` |
| Command | `<Verb><Noun>Command` | `CancelOrderCommand` |
| Query | `Get<Noun>[By<X>]Query`, `List<Noun>Query` | `GetOrderByNumberQuery`, `ListProductsQuery` |
| Handler | `<Command/Query>Handler` | `CancelOrderCommandHandler` |
| Validator | `<Command/Query>Validator` | `CancelOrderCommandValidator` |
| Response DTO | `<Noun>Response`, `<Noun>SummaryResponse`, `<Noun>DetailResponse` | `OrderDetailResponse` |
| Request body (Api) | `<Verb><Noun>Request` | `PlaceOrderRequest` |
| Domain event | `<Noun><PastVerb>DomainEvent` | `OrderPlacedDomainEvent` |
| Domain error class | `<Aggregate>Errors` | `OrderErrors.NotFound` |
| EF configuration | `<Entity>Configuration` | `OrderConfiguration` |
| Endpoint group | `<Module>Endpoints` | `OrdersEndpoints` |
| Test | `MethodName_Scenario_ExpectedResult` | `Cancel_WhenShipped_ReturnsInvalidTransition` |

Không viết tắt trừ các từ phổ biến: `Id`, `Dto`, `Api`, `Db`, `Sku`, `Url`, `Http`, `Json`. Không dùng tiền tố Hungarian, không `Manager`/`Helper`/`Util` làm tên class (tìm tên có nghĩa, ví dụ `SlugGenerator`).

## 3. Cấu trúc một feature (template)

```csharp
// Application/Features/Orders/CancelOrder/CancelOrderCommand.cs
namespace SnackShop.Application.Features.Orders.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, string Reason) : ICommand;
```

```csharp
// Application/Features/Orders/CancelOrder/CancelOrderCommandValidator.cs
public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
```

```csharp
// Application/Features/Orders/CancelOrder/CancelOrderCommandHandler.cs
public sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IStockItemRepository stockRepository,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : ICommandHandler<CancelOrderCommand>
{
    public async Task<Result> Handle(CancelOrderCommand command, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(command.OrderId, ct);
        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(command.OrderId));
        }

        if (!currentUser.IsInRole(Roles.Staff) && order.CustomerId != currentUser.UserId)
        {
            return Result.Failure(OrderErrors.Forbidden);
        }

        var cancelResult = order.Cancel(command.Reason, CancelledBy.FromUser(currentUser), timeProvider.GetUtcNow());
        if (cancelResult.IsFailure)
        {
            return cancelResult;
        }

        await stockRepository.ReleaseForOrderAsync(order, ct);
        return Result.Success();
        // UnitOfWorkBehavior sẽ SaveChanges; domain event OrderCancelled dispatch sau commit.
    }
}
```

Quy tắc:
- **Primary constructor** cho DI. Không `readonly` field thủ công nữa.
- Handler `sealed`, không kế thừa, không chứa `static` helper dùng chung giữa handler (nếu cần → class riêng trong `Common/`).
- Mỗi handler **≤ 60 dòng**. Dài hơn là dấu hiệu logic nên đẩy xuống Domain hoặc tách query.
- Luôn nhận và truyền `CancellationToken ct`.

## 4. Result pattern và xử lý lỗi

### 4.1 `Error` và `Result`

```csharp
public enum ErrorType { Validation, NotFound, Conflict, Forbidden, Unauthorized, Failure }

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
}
```

Mã lỗi (`Code`) dạng `Module.Thing.Reason`, ổn định, frontend có thể map sang message tiếng Việt:

```csharp
public static class OrderErrors
{
    public static Error NotFound(Guid id) => Error.NotFound("Orders.NotFound", $"Order '{id}' was not found.");
    public static readonly Error InvalidStatusTransition = Error.Conflict("Orders.InvalidStatusTransition", "The order cannot move to the requested status.");
    public static readonly Error EmptyOrder = Error.Validation("Orders.Empty", "An order must contain at least one item.");
    public static readonly Error Forbidden = Error.Forbidden("Orders.Forbidden", "You do not have access to this order.");
}
```

### 4.2 Khi nào `Result`, khi nào `throw`

| Tình huống | Làm gì |
|---|---|
| Vi phạm rule nghiệp vụ (hủy đơn đã giao, hết hàng) | `Result.Failure(Error)` |
| Không tìm thấy entity theo id người dùng gửi | `Result.Failure(Errors.NotFound)` |
| Input sai định dạng | FluentValidation (behavior tự trả `ValidationError`) |
| Vi phạm invariant do **bug lập trình** (gọi `AddItem(null)`) | `throw ArgumentException` / `InvalidOperationException` — đây là lỗi dev, không phải lỗi user |
| DB lỗi, mạng lỗi, config thiếu | Để exception nổi lên, `GlobalExceptionHandler` trả 500 + log |
| `DbUpdateConcurrencyException` | Bắt trong `UnitOfWorkBehavior` → `Result.Failure(Error.Conflict("Concurrency", ...))` |

**Cấm**: `catch (Exception) { }` nuốt lỗi; `throw ex;` (mất stack); dùng exception để điều khiển luồng.

### 4.3 Guard trong Domain

```csharp
public Result<ProductVariant> AddVariant(string name, Sku sku, Money price)
{
    if (_variants.Any(v => v.Sku == sku))
    {
        return Result.Failure<ProductVariant>(CatalogErrors.DuplicateSku(sku));
    }

    var variant = new ProductVariant(Id, name, sku, price);
    _variants.Add(variant);
    return variant;
}
```

Domain method public trả `Result`/`Result<T>` khi có thể fail vì nghiệp vụ. Constructor private + static factory `Create(...)` trả `Result<T>`.

## 5. Entities và Value Objects

```csharp
public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    private Order() { } // EF Core

    private Order(Guid id, ...) : base(id) { ... }

    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public static Result<Order> Place(...) { ... Raise(new OrderPlacedDomainEvent(order.Id)); return order; }

    public Result Confirm(DateTimeOffset now, Guid staffId)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }
        ChangeStatus(OrderStatus.Confirmed, now, staffId, note: null);
        Raise(new OrderConfirmedDomainEvent(Id));
        return Result.Success();
    }
}
```

- Setter `private`. Collection expose `IReadOnlyCollection<T>`, backing field `List<T>`.
- Constructor rỗng `private` cho EF Core, có comment `// EF Core`.
- `Guid.CreateVersion7()` cho Id, tạo trong factory (không để DB sinh) để Domain có Id trước khi save.
- Value Object là `record` (hoặc kế thừa `ValueObject` nếu cần so sánh tùy biến), **immutable**, validate trong factory `Create()`.
- **Không** đặt attribute EF (`[Key]`, `[MaxLength]`) trong Domain. Toàn bộ mapping ở `Infrastructure/Persistence/Configurations`.
- Enum lưu DB dạng **string** (`HasConversion<string>()`), dễ đọc, không vỡ khi đổi thứ tự.

## 6. Query handler và projection

```csharp
public sealed class ListProductsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<ListProductsQuery, PagedResponse<ProductSummaryResponse>>
{
    public async Task<Result<PagedResponse<ProductSummaryResponse>>> Handle(ListProductsQuery q, CancellationToken ct)
    {
        var query = db.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(q.CategorySlug))
        {
            query = query.Where(p => p.Category.Slug == q.CategorySlug);
        }

        var projected = query.Select(p => new ProductSummaryResponse(
            p.Id, p.Name, p.Slug.Value,
            p.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault(),
            p.Variants.Where(v => v.IsActive).Min(v => v.Price.Amount),
            p.SoldCount));

        return await projected.ToPagedResponseAsync(q.Page, q.PageSize, ct);
    }
}
```

- Luôn `AsNoTracking()` cho query.
- **Project trong DB** bằng `Select` sang record, không load entity rồi map trong memory.
- Không `Include` rồi `Select` thủ công: projection đã đủ.
- Phân trang bắt buộc cho mọi list; `PageSize` tối đa 100 (validator).
- Không trả entity Domain ra khỏi Application. Luôn là `Response` record.

## 7. Async, DI, lifetime

- `async`/`await` toàn tuyến. Không `.Result`, `.Wait()`, `Task.Run` trong request path.
- Không `async void`. Không `ConfigureAwait(false)` trong ASP.NET Core (không cần).
- Lifetime: `DbContext`, repository, `UnitOfWork`, `CurrentUser` → **Scoped**. `JwtProvider`, `TimeProvider`, options → **Singleton**. Handler do MediatR đăng ký Transient.
- Không inject `IServiceProvider` để resolve tay (service locator) trừ trong factory/background job có `IServiceScopeFactory`.
- Options pattern: `services.AddOptions<JwtOptions>().BindConfiguration("Jwt").ValidateDataAnnotations().ValidateOnStart()`. Không đọc `IConfiguration["..."]` rải rác.

## 8. Logging

- `ILogger<T>` qua DI. Message template, **không** string interpolation:
  ```csharp
  logger.LogInformation("Order {OrderNumber} placed by {CustomerId} total {Total}", order.OrderNumber, order.CustomerId, order.Total.Amount);
  ```
- Mức: `Debug` chi tiết dev; `Information` sự kiện nghiệp vụ quan trọng (đặt đơn, đổi trạng thái, login); `Warning` lỗi nghiệp vụ đáng chú ý (hết hàng liên tục, login fail nhiều); `Error` exception.
- **Không log** mật khẩu, token, số thẻ, nội dung email đầy đủ. SĐT và email mask khi log ở `Information`.
- Dùng `[LoggerMessage]` source generator cho log nóng (behavior, middleware).

## 9. Comment và tài liệu trong code

- Code phải tự giải thích. Comment để nói **tại sao**, không nói **cái gì**.
- XML doc `///` bắt buộc cho: public interface trong `Application/Abstractions`, Domain aggregate root method public, `Error` tĩnh.
- `// TODO(name): ...` phải kèm issue/ticket. CI không chặn nhưng review sẽ hỏi.
- Không để code comment-out trong PR.

## 10. Những điều cấm (sẽ bị reject khi review)

1. Business logic trong Endpoint, Controller, Middleware, hoặc EF Configuration.
2. `DbContext` hoặc `DbSet` dùng trực tiếp trong `Api` hoặc trong command handler (command dùng repository).
3. Entity Domain trả về từ endpoint (phải là Response record).
4. `DateTime.Now` / `DateTime.UtcNow` / `Guid.NewGuid()` trong Domain/Application (dùng `TimeProvider`, `Guid.CreateVersion7()`).
5. `public set` trên entity. Collection public `List<T>`.
6. Magic string/number: mã lỗi, role, policy, route prefix phải là constant (`Roles.Admin`, `Policies.StaffOrAdmin`, `ApiRoutes.V1`).
7. Nuốt exception, `catch` không log, `throw ex`.
8. AutoMapper, generic repository, `dynamic`, reflection trong request path.
9. SQL raw string nối chuỗi. Nếu cần raw SQL → `FromSqlInterpolated` / Dapper với parameter.
10. Hard-code secret, connection string, API key trong code hay `appsettings.json` commit lên git.
11. Thay đổi migration đã merge vào `main`. Luôn tạo migration mới.
12. PR thêm feature mà không có test.

## 11. Checklist tự review trước khi mở PR

- [ ] `dotnet build` 0 warning, `dotnet format --verify-no-changes` pass, `dotnet test` xanh.
- [ ] Đặt tên theo bảng mục 2. Mã lỗi theo dạng `Module.Thing.Reason`.
- [ ] Handler ≤ 60 dòng, logic nghiệp vụ nằm trong Domain.
- [ ] Mọi list endpoint có phân trang. Mọi query `AsNoTracking` + projection.
- [ ] Endpoint có `.WithName`, `.Produces`, `.ProducesProblem`, policy authorization đúng.
- [ ] Không log dữ liệu nhạy cảm.
- [ ] Nếu đổi schema: có migration, có cập nhật `06-database.md` khi cần.
- [ ] Nếu đổi hành vi nghiệp vụ: có cập nhật `03-domain-model.md`.
