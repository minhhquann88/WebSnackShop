# 09 — Chiến lược test

Mục tiêu: **tự tin merge** mà không phải click tay qua UI. Test là một phần của feature, không phải việc làm sau. PR thêm hành vi mà không có test sẽ bị trả lại.

## 1. Kim tự tháp và nơi đặt

| Tầng | Project | Test gì | Công cụ | Tốc độ |
|---|---|---|---|---|
| **Unit – Domain** | `SnackShop.Domain.Tests` | Invariant, state machine, value object, tính tiền | xUnit, Shouldly | ms |
| **Unit – Application** | `SnackShop.Application.Tests` | Handler với repository giả, validator, behavior | xUnit, Shouldly, NSubstitute | ms |
| **Integration – API** | `SnackShop.Api.IntegrationTests` | Endpoint thật → DB Postgres thật, auth, migration, EF mapping | `WebApplicationFactory`, Testcontainers, Respawn | giây |
| **Architecture** | `SnackShop.ArchitectureTests` | Quy tắc phụ thuộc, naming, sealed handler | NetArchTest.Rules | ms |
| **E2E** (Phase 3) | `frontend/e2e` | Luồng đặt hàng từ UI | Playwright | phút |

Tỷ lệ mục tiêu: nhiều unit Domain, vừa phải Application, mỗi endpoint ≥ 2 integration test.

## 2. Thư viện (pin trong `Directory.Packages.props`)

| Thư viện | Dùng cho | Ghi chú |
|---|---|---|
| `xunit.v3` + `xunit.runner.visualstudio` | Framework | Chạy trên **Microsoft.Testing.Platform**; `global.json` có `"test": { "runner": "Microsoft.Testing.Platform" }` để `dotnet test` dùng runner mới (bắt buộc với .NET 10 SDK) |
| `Shouldly` | Assertion | Chọn thay FluentAssertions vì FA v8 chuyển license thương mại |
| `NSubstitute` | Mock interface | Chỉ mock interface của `Application/Abstractions` và repository; không mock DbContext |
| `Bogus` | Dữ liệu giả | Seed cố định `Randomizer.Seed = new Random(1234)` |
| `Testcontainers.PostgreSql` | Postgres thật cho integration | Yêu cầu Docker trên máy dev và CI |
| `Respawn` | Reset DB giữa các test | Nhanh hơn recreate |
| `Microsoft.AspNetCore.Mvc.Testing` | `WebApplicationFactory` | |
| `NetArchTest.Rules` | Architecture test | |
| `Microsoft.Testing.Extensions.CodeCoverage` | Coverage | `dotnet test --coverage --coverage-output-format cobertura` (coverlet.collector không chạy trên MTP) |

## 3. Naming và cấu trúc

- Thư mục test **phản chiếu** thư mục source: `Domain.Tests/Ordering/OrderTests.cs` ↔ `Domain/Ordering/Order.cs`.
- Tên method: `Method_Scenario_Expected`:
  ```csharp
  [Fact]
  public void Confirm_WhenPending_ChangesStatusAndRaisesEvent() { }

  [Fact]
  public void Confirm_WhenAlreadyShipped_ReturnsInvalidStatusTransition() { }

  [Theory]
  [InlineData(0)] [InlineData(100)]
  public void Create_WithQuantityOutOfRange_ReturnsValidationError(int qty) { }
  ```
- Cấu trúc **Arrange / Act / Assert** có dòng trống phân cách, không comment `// Arrange` (tự hiểu).
- Một assert chính mỗi test (có thể nhiều `Should` cho cùng đối tượng).
- Test data builder trong `tests/SnackShop.TestCommon/Builders/`: `new OrderBuilder().WithItems(2).Pending().Build()`. Không copy-paste khối tạo entity dài.

## 4. Unit test Domain

Kiểm **mọi** nhánh của state machine và mọi `Error` trong `*Errors`. Ví dụ bắt buộc có:

- `Order.Place` với 0 item → `OrderErrors.EmptyOrder`.
- `Order.Place` tính `Total = Subtotal + ShippingFee - Discount`.
- Toàn bộ bảng chuyển trạng thái ở [03-domain-model.md](03-domain-model.md#state-machine-đơn-hàng): mỗi ô hợp lệ 1 test, mỗi nhóm không hợp lệ 1 `Theory`.
- `StockItem.Reserve` quá `Available` → `InsufficientStock`; `Reserve` tạo `StockMovement`.
- `Money` cộng khác currency → throw (bug dev).
- `Slug.FromText("Khô Gà Lá Chanh 200g")` → `"kho-ga-la-chanh-200g"`.
- Domain event được raise đúng loại và đúng số lần.

Domain test **không** cần DI, không cần mock. Nếu cần mock để test Domain là Domain đang phụ thuộc sai.

## 5. Unit test Application

- Handler: mock repository (`NSubstitute`), truyền `FakeTimeProvider` (từ `Microsoft.Extensions.TimeProvider.Testing`), `ICurrentUser` giả.
- Kiểm: trả `Result` đúng, gọi repository đúng method, không gọi khi fail sớm.
- Validator: dùng `TestValidate` của FluentValidation, mỗi rule ít nhất 1 case fail và 1 pass.
- Query handler **không** unit test (chỉ projection EF). Test chúng ở integration.
- Behavior: `ValidationBehavior` trả `ValidationError` khi validator fail và không gọi `next`.

## 6. Integration test API

### Hạ tầng

```csharp
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:17-alpine").Build();
    private Respawner _respawner = default!;

    public async ValueTask InitializeAsync()
    {
        await _db.StartAsync();
        // Migrate một lần
        // Respawner để reset giữa test
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureTestServices(services =>
        {
            services.ReplaceConnectionString(_db.GetConnectionString());
            services.Replace<IEmailSender, FakeEmailSender>();        // không gửi mail thật
            services.Replace<IGoogleTokenVerifier, FakeGoogleVerifier>();
            services.Replace<TimeProvider>(new FakeTimeProvider(...));
        });
    }

    public Task ResetDatabaseAsync() => _respawner.ResetAsync(...);
}
```

- Một container Postgres cho **cả** test assembly (`ICollectionFixture`), reset bằng Respawn trước mỗi test (nhanh ~50ms).
- Migration chạy thật → test cũng kiểm migration chạy được từ DB rỗng.
- Thay **chỉ** adapter ra ngoài (email, Google, thời gian, storage). Không thay DbContext, không thay MediatR.
- Helper: `client.AsCustomerAsync()`, `client.AsStaffAsync()`, `client.AsAdminAsync()` tạo user + lấy JWT thật qua `/auth/login`.
- Helper: `factory.SeedAsync(product with stock)` dùng builder, ghi thẳng qua `ApplicationDbContext` trong scope riêng.

### Mỗi endpoint bắt buộc có

1. Happy path: đúng status code, đúng body (so sánh field quan trọng, không snapshot toàn bộ).
2. Ít nhất 1 lỗi nghiệp vụ (409/404) và kiểm `ProblemDetails.Title == Error.Code`.
3. Nếu cần auth: 401 khi không token, 403 khi sai role.
4. Nếu có validation: 1 case 400 với `errors` chứa key field đúng.

Luồng quan trọng phải có test xuyên suốt: **đặt hàng → tồn kho reserve → confirm → tồn giảm → hủy → tồn trả**; **refresh token rotation + reuse detection**; **guest cart merge**.

## 7. Architecture test

```csharp
[Fact]
public void Domain_ShouldNotDependOn_AnyOtherLayer()
    => Types.InAssembly(DomainAssembly)
        .ShouldNot().HaveDependencyOnAny("SnackShop.Application", "SnackShop.Infrastructure", "SnackShop.Api",
                                          "Microsoft.EntityFrameworkCore", "MediatR")
        .GetResult().IsSuccessful.ShouldBeTrue();

[Fact]
public void Application_ShouldNotDependOn_InfrastructureOrApi() { }

[Fact]
public void Handlers_ShouldBeSealed_AndEndWithHandler() { }

[Fact]
public void Commands_ShouldHaveValidator() { }   // mọi ICommand có 1 AbstractValidator tương ứng

[Fact]
public void Endpoints_ShouldNotReference_DbContext() { }

[Fact]
public void DomainEntities_ShouldHavePrivateSetters() { }
```

Chạy cùng unit test, ms. Khi thêm quy tắc mới vào `04-coding-conventions.md` mà máy kiểm được → thêm test ở đây.

## 8. Coverage và chất lượng

- Mục tiêu: Domain ≥ 90 %, Application ≥ 80 %, tổng ≥ 75 %. CI báo cáo (ReportGenerator → summary trong PR), **không** fail build vì coverage ở MVP; fail khi ổn định (Phase 3).
- Test phải **deterministic**: không `DateTime.Now`, không random không seed, không phụ thuộc thứ tự, không sleep.
- Test chậm (> 2s) đánh `[Trait("Category","Slow")]`, CI vẫn chạy, dev có thể lọc.
- Không `Skip` test quá 1 sprint. Xóa hoặc sửa.

## 9. Lệnh

```bash
dotnet test                                                      # tất cả (chạy từ backend/)
dotnet test --project tests/SnackShop.Domain.Tests               # nhanh, không cần Docker
dotnet test --filter-trait "Category!=Slow"
dotnet test --coverage --coverage-output-format cobertura --results-directory ./TestResults
reportgenerator -reports:TestResults/**/*.cobertura.xml -targetdir:coverage
```

`dotnet test` trên Microsoft.Testing.Platform nhận tham số trực tiếp (`--report-trx`, `--coverage`, `--filter-trait`), không dùng `--logger`/`--collect` kiểu VSTest cũ. Test method không cần hậu tố `Async` (ngoại lệ trong `.editorconfig` cho `backend/tests/**`).

Integration test cần Docker Desktop đang chạy trên máy dev.
