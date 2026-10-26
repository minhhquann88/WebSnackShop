using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;
using SnackShop.Domain.Customers;
using SnackShop.Domain.Inventory;
using SnackShop.Domain.Ordering;

namespace SnackShop.Domain.Tests;

/// <summary>Small builders so tests state only what matters to them.</summary>
internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 10, 22, 3, 0, 0, TimeSpan.Zero);
    public static readonly StockContext Ctx = new(Now, Guid.CreateVersion7());
    public static readonly OrderActor Staff = OrderActor.Staff(Guid.CreateVersion7());

    public static VariantSpec Variant(string sku = "SKU-001", decimal price = 50_000m, decimal? compareAt = null, string name = "") =>
        new(name, Sku.Create(sku).Value, Money.Vnd(price), compareAt is null ? null : Money.Vnd(compareAt.Value), 200, 0);

    public static ProductDetails Details(string name = "Khô gà lá chanh") => new(name, "Ngon", null, null, null, "Brand", 180);

    public static Product Product(string sku = "SKU-001") =>
        Domain.Catalog.Product.Create(Guid.CreateVersion7(), Details(), Slug.FromText("Khô gà lá chanh").Value, Variant(sku)).Value;

    public static AddressInfo Address() => new("Nguyễn Văn A", "0912345678", "12 Lê Lợi", "Bến Nghé", "Quận 1", "TP HCM");

    public static OrderLine Line(int quantity = 2, decimal price = 50_000m) =>
        new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Khô gà", "Default", "SKU-001", null, Money.Vnd(price), quantity);

    public static Order Order(PaymentMethod method = PaymentMethod.Cod, params OrderLine[] lines)
    {
        var data = new PlaceOrderData(OrderNumber.Create(Now, 42), Guid.CreateVersion7(), "A@Example.com", Address(), method, null, null);
        return Domain.Ordering.Order.Place(data, lines.Length == 0 ? [Line()] : lines, Money.Vnd(25_000m), Now).Value;
    }
}
