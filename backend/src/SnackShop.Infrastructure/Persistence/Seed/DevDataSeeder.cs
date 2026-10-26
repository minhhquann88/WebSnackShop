using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;
using SnackShop.Domain.Inventory;

namespace SnackShop.Infrastructure.Persistence.Seed;

/// <summary>
/// Development-only sample catalog (runs only when the catalog is empty). Deterministic, so screenshots and
/// manual tests look the same on every machine. Images are placeholders from placehold.co.
/// </summary>
public sealed partial class DevDataSeeder(ApplicationDbContext db, TimeProvider timeProvider, ILogger<DevDataSeeder> logger)
{
    private sealed record SeedProduct(string Name, string Brand, string Short, decimal Price, string? Variant2, decimal? Price2, bool Featured);

    private static readonly (string Name, string Description, SeedProduct[] Products)[] Catalog =
    [
        ("Khô gà & khô bò", "Khô gà lá chanh, khô bò, khô heo cháy tỏi.",
        [
            new("Khô gà lá chanh", "Bếp Nhà Lá", "Khô gà xé sợi thơm lá chanh, cay nhẹ.", 65_000m, "Hũ 500g", 155_000m, true),
            new("Khô bò miếng", "Bếp Nhà Lá", "Khô bò miếng mềm, vị cay ngọt.", 95_000m, "Túi 250g", 185_000m, true),
            new("Khô heo cháy tỏi", "Bếp Nhà Lá", "Heo khô cháy tỏi giòn rụm.", 70_000m, null, null, false),
        ]),
        ("Bánh tráng", "Bánh tráng trộn, bánh tráng nướng, bánh tráng cuộn.",
        [
            new("Bánh tráng trộn Tây Ninh", "Tây Ninh Food", "Bánh tráng trộn đủ topping, sốt me.", 25_000m, "Combo 5 bịch", 115_000m, true),
            new("Bánh tráng phơi sương", "Tây Ninh Food", "Bánh tráng dẻo, cuốn thịt luộc.", 35_000m, null, null, false),
            new("Bánh tráng bơ cuộn", "Tây Ninh Food", "Bánh tráng cuộn bơ béo ngậy.", 30_000m, null, null, false),
        ]),
        ("Hạt & trái cây sấy", "Hạt điều, hạnh nhân, mít sấy, chuối sấy.",
        [
            new("Hạt điều rang muối", "Bình Phước Nuts", "Hạt điều loại A rang muối vỏ lụa.", 120_000m, "Hũ 500g", 230_000m, true),
            new("Mít sấy giòn", "Đà Lạt Farm", "Mít sấy giòn tự nhiên, không đường.", 45_000m, null, null, false),
            new("Hạt hướng dương vị dừa", "Đà Lạt Farm", "Hướng dương rang vị dừa thơm.", 20_000m, null, null, false),
        ]),
        ("Kẹo & bánh ngọt", "Kẹo dừa, bánh pía, mè xửng.",
        [
            new("Kẹo dừa Bến Tre", "Bến Tre Sweets", "Kẹo dừa sữa mềm truyền thống.", 40_000m, "Vị lá dứa", 42_000m, false),
            new("Bánh pía sầu riêng", "Sóc Trăng Bakery", "Bánh pía nhân sầu riêng trứng muối.", 85_000m, null, null, true),
            new("Mè xửng Huế", "Huế Xưa", "Mè xửng dẻo, thơm mè rang.", 35_000m, null, null, false),
        ]),
        ("Đồ uống", "Trà, nước ép, sữa hạt đóng chai.",
        [
            new("Trà tắc mật ong", "Fresh Drinks", "Trà tắc mật ong đóng chai 500ml.", 22_000m, "Thùng 12 chai", 240_000m, false),
            new("Sữa hạt óc chó", "Fresh Drinks", "Sữa hạt óc chó ít đường 330ml.", 28_000m, null, null, false),
        ]),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.CategorySet.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        var context = new StockContext(timeProvider.GetUtcNow(), null);
        var skuNumber = 1;
        var categoryOrder = 0;

        foreach (var (categoryName, description, products) in Catalog)
        {
            var category = Category.Create(categoryName, Slug.FromText(categoryName).Value, description, null, categoryOrder++);
            db.CategorySet.Add(category);

            foreach (var p in products)
            {
                var details = new ProductDetails(p.Name, p.Short, $"{p.Short} Sản phẩm mẫu cho môi trường phát triển.",
                    "Xem trên bao bì", "Việt Nam", p.Brand, 180);
                var first = new VariantSpec(p.Variant2 is null ? string.Empty : "Gói tiêu chuẩn",
                    Sku.Create($"SS-{skuNumber++:D4}").Value, Money.Vnd(p.Price), Money.Vnd(decimal.Round(p.Price * 1.2m / 1000m) * 1000m), 200, 0);

                var product = Product.Create(category.Id, details, Slug.FromText(p.Name).Value, first).Value;
                if (p.Variant2 is { } name && p.Price2 is { } price2)
                {
                    product.AddVariant(new VariantSpec(name, Sku.Create($"SS-{skuNumber++:D4}").Value, Money.Vnd(price2), null, 500, 1));
                }

                product.AddImage($"https://placehold.co/800x800/png?text={Uri.EscapeDataString(TextNormalizer.RemoveDiacritics(p.Name))}", p.Name);
                product.SetVisibility(isActive: true, isFeatured: p.Featured);
                db.ProductSet.Add(product);

                foreach (var variant in product.Variants)
                {
                    var stock = StockItem.CreateFor(variant.Id);
                    stock.Receive(skuNumber % 4 == 0 ? 5 : 100, "Dev seed", context);
                    db.StockItemSet.Add(stock);
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        LogSeeded(logger, Catalog.Length);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded development catalog with {CategoryCount} categories")]
    private static partial void LogSeeded(ILogger logger, int categoryCount);
}
