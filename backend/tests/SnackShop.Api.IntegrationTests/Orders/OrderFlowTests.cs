using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SnackShop.Api.IntegrationTests.Infrastructure;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Cart;
using SnackShop.Application.Features.Orders;
using SnackShop.Application.Features.Orders.EstimateOrder;
using SnackShop.Application.Features.Orders.PlaceOrder;
using SnackShop.Application.Features.Products;
using SnackShop.Application.Features.Reports;
using SnackShop.Domain.Ordering;

namespace SnackShop.Api.IntegrationTests.Orders;

/// <summary>Checkout, the order state machine and its stock effects, end to end against PostgreSQL.</summary>
[Collection(ApiCollection.Name)]
public sealed class OrderFlowTests(ApiFactory factory) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await factory.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static async Task<AdminVariantResponse> StockAsync(HttpClient admin, Guid productId) =>
        (await (await admin.GetAsync($"/api/v1/admin/products/{productId}", TestApi.Ct)).ReadAsync<AdminProductDetailResponse>()).Variants.Single();

    [Fact]
    public async Task GuestCod_FullLifecycle_MovesStockAndPayment()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(price: 50_000m, stock: 10);
        var variantId = product.Variants.Single().Id;
        var guest = factory.CreateClient();

        var placed = await (await guest.PostJsonAsync("/api/v1/orders", TestApi.OrderBody(variantId, 3))
            .ShouldBeAsync(HttpStatusCode.Created)).ReadAsync<PlaceOrderResponse>();
        placed.Total.Amount.ShouldBe(175_000m); // 150k + 25k default shipping
        placed.BankTransfer.ShouldBeNull();

        var afterPlace = await StockAsync(admin, product.Id);
        (afterPlace.QuantityOnHand, afterPlace.QuantityReserved).ShouldBe((10, 3));

        foreach (var action in new[] { "confirm", "prepare", "ship", "deliver" })
        {
            await admin.PostJsonAsync($"/api/v1/admin/orders/{placed.OrderId}/{action}", new { }).ShouldBeAsync(HttpStatusCode.NoContent);
        }

        var afterDeliver = await StockAsync(admin, product.Id);
        (afterDeliver.QuantityOnHand, afterDeliver.QuantityReserved).ShouldBe((7, 0));

        var order = await (await admin.GetAsync($"/api/v1/admin/orders/{placed.OrderId}", TestApi.Ct)).ReadAsync<AdminOrderDetailResponse>();
        order.Status.ShouldBe(OrderStatus.Delivered);
        order.Payment.Status.ShouldBe(PaymentStatus.Paid);
        order.History.Count.ShouldBe(5);
        order.Actions.ShouldBe(new AdminOrderActionsResponse(false, false, false, false, false, false, false));

        var soldCount = (await (await admin.GetAsync($"/api/v1/admin/products/{product.Id}", TestApi.Ct)).ReadAsync<AdminProductDetailResponse>()).SoldCount;
        soldCount.ShouldBe(3);

        var tracked = await (await guest.GetAsync($"/api/v1/orders/track?orderNumber={placed.OrderNumber}&phone=0912345678", TestApi.Ct)
            .ShouldBeAsync(HttpStatusCode.OK)).ReadAsync<OrderDetailResponse>();
        tracked.Timeline.Select(t => t.Status).Last().ShouldBe(OrderStatus.Delivered);
        tracked.CanCancel.ShouldBeFalse();
    }

    [Fact]
    public async Task BankTransfer_RequiresPaymentBeforeConfirm_AndReturnsQrInstructions()
    {
        var admin = await factory.LoginAdminAsync();
        await admin.PutJsonAsync("/api/v1/admin/settings", new
        {
            shopName = "Snack",
            shopPhone = "0912345678",
            shopAddress = "HCM",
            flatShippingFee = new { amount = 20_000m },
            freeShippingThreshold = (object?)null,
            bankName = "Vietcombank",
            bankAccountNumber = "0011002233",
            bankAccountName = "NGUYEN VAN A",
            bankBin = "970436",
            unpaidBankTransferExpiryHours = 24,
        }).ShouldBeAsync(HttpStatusCode.NoContent);

        var product = await admin.CreateProductAsync(price: 100_000m, stock: 5);

        var placed = await (await factory.CreateClient().PostJsonAsync("/api/v1/orders",
            TestApi.OrderBody(product.Variants.Single().Id, 1, "BankTransfer")).ShouldBeAsync(HttpStatusCode.Created)).ReadAsync<PlaceOrderResponse>();

        var bank = placed.BankTransfer.ShouldNotBeNull();
        bank.Amount.Amount.ShouldBe(120_000m);
        bank.TransferContent.ShouldBe(placed.OrderNumber.Replace("-", string.Empty, StringComparison.Ordinal));
        bank.QrImageUrl.ShouldStartWith("https://img.vietqr.io/image/970436-0011002233-compact2.png?amount=120000");
        factory.Emails.Sent.ShouldContain(e => e.Subject.Contains(placed.OrderNumber) && e.HtmlBody.Contains("img.vietqr.io"));

        var refused = await admin.PostJsonAsync($"/api/v1/admin/orders/{placed.OrderId}/confirm", new { })
            .ShouldBeAsync(HttpStatusCode.Conflict);
        (await refused.ProblemAsync()).Title.ShouldBe("Orders.PaymentRequired");

        await admin.PostJsonAsync($"/api/v1/admin/orders/{placed.OrderId}/confirm", new { markPaid = true, transactionRef = "FT26102200001" })
            .ShouldBeAsync(HttpStatusCode.NoContent);
        var order = await (await admin.GetAsync($"/api/v1/admin/orders/{placed.OrderId}", TestApi.Ct)).ReadAsync<AdminOrderDetailResponse>();
        order.Payment.TransactionRef.ShouldBe("FT26102200001");
        order.Payment.Status.ShouldBe(PaymentStatus.Paid);
    }

    [Fact]
    public async Task Cancel_FromConfirmed_ReturnsStock_FromPendingReleasesReservation()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(stock: 10);
        var variantId = product.Variants.Single().Id;
        var guest = factory.CreateClient();

        var a = await (await guest.PostJsonAsync("/api/v1/orders", TestApi.OrderBody(variantId, 2))).ReadAsync<PlaceOrderResponse>();
        var b = await (await guest.PostJsonAsync("/api/v1/orders", TestApi.OrderBody(variantId, 3))).ReadAsync<PlaceOrderResponse>();
        await admin.PostJsonAsync($"/api/v1/admin/orders/{a.OrderId}/confirm", new { }).ShouldBeAsync(HttpStatusCode.NoContent);
        (await StockAsync(admin, product.Id)).QuantityOnHand.ShouldBe(8);

        await admin.PostJsonAsync($"/api/v1/admin/orders/{a.OrderId}/cancel", new { reason = "Khách đổi ý" }).ShouldBeAsync(HttpStatusCode.NoContent);
        await admin.PostJsonAsync($"/api/v1/admin/orders/{b.OrderId}/cancel", new { reason = "Trùng đơn" }).ShouldBeAsync(HttpStatusCode.NoContent);

        var stock = await StockAsync(admin, product.Id);
        (stock.QuantityOnHand, stock.QuantityReserved).ShouldBe((10, 0));

        var cancelWithoutReason = await admin.PostJsonAsync($"/api/v1/admin/orders/{a.OrderId}/cancel", new { reason = "" })
            .ShouldBeAsync(HttpStatusCode.BadRequest);
        cancelWithoutReason.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task OutOfStock_Returns409_AndNothingIsReserved()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(stock: 2);

        var response = await factory.CreateClient().PostJsonAsync("/api/v1/orders", TestApi.OrderBody(product.Variants.Single().Id, 3))
            .ShouldBeAsync(HttpStatusCode.Conflict);

        (await response.ProblemAsync()).Title.ShouldBe("Inventory.InsufficientStock");
        (await StockAsync(admin, product.Id)).QuantityReserved.ShouldBe(0);
        (await factory.WithDbAsync(db => db.OrderSet.CountAsync(TestApi.Ct))).ShouldBe(0);
    }

    [Fact]
    public async Task ConcurrentCheckouts_ForLastUnit_SellExactlyOne()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(stock: 1);
        var variantId = product.Variants.Single().Id;

        var attempts = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            factory.CreateClient().PostJsonAsync("/api/v1/orders", TestApi.OrderBody(variantId, 1))));

        attempts.Count(r => r.StatusCode == HttpStatusCode.Created).ShouldBe(1);
        attempts.Where(r => r.StatusCode != HttpStatusCode.Created).ShouldAllBe(r => r.StatusCode == HttpStatusCode.Conflict);
        var stock = await StockAsync(admin, product.Id);
        (stock.QuantityOnHand, stock.QuantityReserved).ShouldBe((1, 1));
    }

    [Fact]
    public async Task IdempotencyKey_ReturnsSameOrder()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(stock: 10);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Idempotency-Key", "checkout-abc-123");

        var first = await (await client.PostJsonAsync("/api/v1/orders", TestApi.OrderBody(product.Variants.Single().Id, 2))).ReadAsync<PlaceOrderResponse>();
        var retry = await (await client.PostJsonAsync("/api/v1/orders", TestApi.OrderBody(product.Variants.Single().Id, 2))
            .ShouldBeAsync(HttpStatusCode.Created)).ReadAsync<PlaceOrderResponse>();

        retry.OrderNumber.ShouldBe(first.OrderNumber);
        (await StockAsync(admin, product.Id)).QuantityReserved.ShouldBe(2);
    }

    [Fact]
    public async Task HiddenProduct_CannotBeOrdered()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(stock: 10);
        await admin.PatchJsonAsync($"/api/v1/admin/products/{product.Id}/status", new { isActive = false, isFeatured = false });

        var response = await factory.CreateClient().PostJsonAsync("/api/v1/orders", TestApi.OrderBody(product.Variants.Single().Id))
            .ShouldBeAsync(HttpStatusCode.Conflict);

        (await response.ProblemAsync()).Title.ShouldBe("Orders.ProductUnavailable");
    }

    [Fact]
    public async Task Estimate_ComputesFreeShippingAndFlagsUnavailable()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(price: 160_000m, stock: 5);
        var client = factory.CreateClient();

        var estimate = await (await client.PostJsonAsync("/api/v1/orders/estimate", new
        {
            items = new[] { new { variantId = product.Variants.Single().Id, quantity = 2 }, new { variantId = Guid.NewGuid(), quantity = 1 } },
        }).ShouldBeAsync(HttpStatusCode.OK)).ReadAsync<OrderEstimateResponse>();

        estimate.Subtotal.Amount.ShouldBe(320_000m);
        estimate.ShippingFee.Amount.ShouldBe(0m); // default free-shipping threshold is 300k
        estimate.CanPlaceOrder.ShouldBeFalse();
        estimate.UnavailableVariantIds.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Customer_CartCheckout_ClearsOrderedLines_AndCanCancelOwnPendingOrder()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(stock: 10);
        var variantId = product.Variants.Single().Id;
        var (customer, auth) = await factory.RegisterCustomerAsync();

        await customer.PostJsonAsync("/api/v1/me/cart/items", new { variantId, quantity = 2 }).ShouldBeAsync(HttpStatusCode.NoContent);
        var cart = await (await customer.GetAsync("/api/v1/me/cart", TestApi.Ct)).ReadAsync<CartResponse>();
        cart.ItemCount.ShouldBe(2);

        var placed = await (await customer.PostJsonAsync("/api/v1/orders", TestApi.OrderBody(variantId, 2, email: auth.User.Email))
            .ShouldBeAsync(HttpStatusCode.Created)).ReadAsync<PlaceOrderResponse>();
        (await (await customer.GetAsync("/api/v1/me/cart", TestApi.Ct)).ReadAsync<CartResponse>()).ItemCount.ShouldBe(0);

        var mine = await (await customer.GetAsync("/api/v1/me/orders", TestApi.Ct)).ReadAsync<PagedResponse<OrderSummaryResponse>>();
        mine.Items.ShouldHaveSingleItem().OrderNumber.ShouldBe(placed.OrderNumber);

        var (other, _) = await factory.RegisterCustomerAsync();
        await other.GetAsync($"/api/v1/me/orders/{placed.OrderNumber}", TestApi.Ct).ShouldBeAsync(HttpStatusCode.NotFound);
        await other.PostJsonAsync($"/api/v1/me/orders/{placed.OrderNumber}/cancel", new { }).ShouldBeAsync(HttpStatusCode.NotFound);

        await customer.PostJsonAsync($"/api/v1/me/orders/{placed.OrderNumber}/cancel", new { reason = "Đổi ý" }).ShouldBeAsync(HttpStatusCode.NoContent);
        (await StockAsync(admin, product.Id)).QuantityReserved.ShouldBe(0);
        var cancelled = await (await customer.GetAsync($"/api/v1/me/orders/{placed.OrderNumber}", TestApi.Ct)).ReadAsync<OrderDetailResponse>();
        cancelled.Status.ShouldBe(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task GuestCartMerge_CombinesQuantities_AndSkipsUnknownVariants()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(stock: 10);
        var variantId = product.Variants.Single().Id;
        var (customer, _) = await factory.RegisterCustomerAsync();
        await customer.PostJsonAsync("/api/v1/me/cart/items", new { variantId, quantity = 1 });

        await customer.PostJsonAsync("/api/v1/me/cart/merge", new
        {
            items = new[] { new { variantId, quantity = 2 }, new { variantId = Guid.NewGuid(), quantity = 1 } },
        }).ShouldBeAsync(HttpStatusCode.NoContent);

        var cart = await (await customer.GetAsync("/api/v1/me/cart", TestApi.Ct)).ReadAsync<CartResponse>();
        cart.Items.ShouldHaveSingleItem().Quantity.ShouldBe(3);
        cart.Subtotal.Amount.ShouldBe(150_000m);
    }

    [Fact]
    public async Task ExpireJob_CancelsOldUnpaidTransfers_AndReleasesStock()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(stock: 5);
        var placed = await (await factory.CreateClient().PostJsonAsync("/api/v1/orders",
            TestApi.OrderBody(product.Variants.Single().Id, 2, "BankTransfer"))).ReadAsync<PlaceOrderResponse>();

        await factory.WithDbAsync(db => db.OrderSet.Where(o => o.Id == placed.OrderId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.PlacedAt, o => o.PlacedAt.AddHours(-25)), TestApi.Ct));

        var job = ActivatorUtilities.CreateInstance<SnackShop.Infrastructure.BackgroundJobs.MaintenanceJob>(factory.Services);
        await job.RunOnceAsync(TestApi.Ct);

        var order = await (await admin.GetAsync($"/api/v1/admin/orders/{placed.OrderId}", TestApi.Ct)).ReadAsync<AdminOrderDetailResponse>();
        order.Status.ShouldBe(OrderStatus.Cancelled);
        order.CancelledBy.ShouldBe(ActorType.System);
        (await StockAsync(admin, product.Id)).QuantityReserved.ShouldBe(0);
    }

    [Fact]
    public async Task Track_WithWrongPhone_Returns404()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync();
        var placed = await (await factory.CreateClient().PostJsonAsync("/api/v1/orders", TestApi.OrderBody(product.Variants.Single().Id)))
            .ReadAsync<PlaceOrderResponse>();

        await factory.CreateClient().GetAsync($"/api/v1/orders/track?orderNumber={placed.OrderNumber}&phone=0999999999", TestApi.Ct)
            .ShouldBeAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvalidCheckoutInput_Returns400WithFieldErrors()
    {
        var response = await factory.CreateClient().PostJsonAsync("/api/v1/orders", new
        {
            items = new[] { new { variantId = Guid.NewGuid(), quantity = 0 } },
            contactEmail = "not-an-email",
            shippingAddress = TestApi.Address(phone: "123"),
            paymentMethod = "Cod",
        }).ShouldBeAsync(HttpStatusCode.BadRequest);

        var problem = await response.ReadAsync<Microsoft.AspNetCore.Mvc.ValidationProblemDetails>();
        problem.Errors.Keys.ShouldContain("contactEmail");
        problem.Errors.Keys.ShouldContain("shippingAddress.phone");
        problem.Errors.Keys.ShouldContain("items[0].quantity");
    }

    [Fact]
    public async Task Dashboard_ReflectsDeliveredRevenue()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(price: 40_000m, stock: 10);
        var placed = await (await factory.CreateClient().PostJsonAsync("/api/v1/orders", TestApi.OrderBody(product.Variants.Single().Id, 2)))
            .ReadAsync<PlaceOrderResponse>();
        foreach (var action in new[] { "confirm", "prepare", "ship", "deliver" })
        {
            await admin.PostJsonAsync($"/api/v1/admin/orders/{placed.OrderId}/{action}", new { });
        }

        var dashboard = await (await admin.GetAsync("/api/v1/admin/reports/dashboard", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK))
            .ReadAsync<DashboardResponse>();

        dashboard.RevenueToday.Amount.ShouldBe(105_000m);
        dashboard.OrdersPlacedToday.ShouldBe(1);
        dashboard.OrdersByStatus.Single(s => s.Status == OrderStatus.Delivered).Count.ShouldBe(1);

        var top = await (await admin.GetAsync($"/api/v1/admin/reports/top-products?from={DateTime.UtcNow.AddDays(-1):yyyy-MM-dd}&to={DateTime.UtcNow.AddDays(1):yyyy-MM-dd}", TestApi.Ct)
            .ShouldBeAsync(HttpStatusCode.OK)).ReadAsync<List<TopProductResponse>>();
        top.ShouldHaveSingleItem().QuantitySold.ShouldBe(2);
    }

    [Fact]
    public async Task IdempotencyKey_FromDifferentBuyer_IsRejected()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(stock: 10);
        var variantId = product.Variants.Single().Id;
        var first = factory.CreateClient();
        first.DefaultRequestHeaders.Add("Idempotency-Key", "shared-key-1");
        var second = factory.CreateClient();
        second.DefaultRequestHeaders.Add("Idempotency-Key", "shared-key-1");

        await first.PostJsonAsync("/api/v1/orders", TestApi.OrderBody(variantId, email: "a@example.com")).ShouldBeAsync(HttpStatusCode.Created);
        var response = await second.PostJsonAsync("/api/v1/orders", TestApi.OrderBody(variantId, email: "b@example.com"))
            .ShouldBeAsync(HttpStatusCode.Conflict);

        (await response.ProblemAsync()).Title.ShouldBe("Orders.IdempotencyKeyReused");
    }

    [Fact]
    public async Task PaidOrder_CustomerCannotCancel_StaffCancelLeavesRefundPending()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(stock: 10);
        var (customer, auth) = await factory.RegisterCustomerAsync();
        var placed = await (await customer.PostJsonAsync("/api/v1/orders",
            TestApi.OrderBody(product.Variants.Single().Id, 1, "BankTransfer", auth.User.Email))).ReadAsync<PlaceOrderResponse>();
        await admin.PostJsonAsync($"/api/v1/admin/orders/{placed.OrderId}/payment/mark-paid", new { transactionRef = "FT1" })
            .ShouldBeAsync(HttpStatusCode.NoContent);

        var refused = await customer.PostJsonAsync($"/api/v1/me/orders/{placed.OrderNumber}/cancel", new { reason = "Đổi ý" })
            .ShouldBeAsync(HttpStatusCode.Conflict);
        (await refused.ProblemAsync()).Title.ShouldBe("Orders.PaidOrderNeedsStaff");

        await admin.PostJsonAsync($"/api/v1/admin/orders/{placed.OrderId}/cancel", new { reason = "Khách nhờ hủy" }).ShouldBeAsync(HttpStatusCode.NoContent);
        var cancelled = await (await admin.GetAsync($"/api/v1/admin/orders/{placed.OrderId}", TestApi.Ct)).ReadAsync<AdminOrderDetailResponse>();
        cancelled.Payment.Status.ShouldBe(PaymentStatus.RefundPending);
        cancelled.Actions.CanMarkRefunded.ShouldBeTrue();

        await admin.PostJsonAsync($"/api/v1/admin/orders/{placed.OrderId}/payment/mark-refunded", new { transactionRef = "FT-REFUND" })
            .ShouldBeAsync(HttpStatusCode.NoContent);
        var refunded = await (await admin.GetAsync($"/api/v1/admin/orders/{placed.OrderId}", TestApi.Ct)).ReadAsync<AdminOrderDetailResponse>();
        refunded.Payment.Status.ShouldBe(PaymentStatus.Refunded);
    }

    [Fact]
    public async Task AdminOrderList_AcceptsDatesWithVietnamOffset()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync();
        await factory.CreateClient().PostJsonAsync("/api/v1/orders", TestApi.OrderBody(product.Variants.Single().Id));
        var from = Uri.EscapeDataString(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).AddDays(-1).ToString("o"));

        var page = await (await admin.GetAsync($"/api/v1/admin/orders?from={from}", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK))
            .ReadAsync<PagedResponse<AdminOrderSummaryResponse>>();

        page.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task StaffEndpoints_RequireStaffRole()
    {
        var (customer, _) = await factory.RegisterCustomerAsync();

        await customer.GetAsync("/api/v1/admin/orders", TestApi.Ct).ShouldBeAsync(HttpStatusCode.Forbidden);
        await factory.CreateClient().GetAsync("/api/v1/admin/orders", TestApi.Ct).ShouldBeAsync(HttpStatusCode.Unauthorized);
    }
}
