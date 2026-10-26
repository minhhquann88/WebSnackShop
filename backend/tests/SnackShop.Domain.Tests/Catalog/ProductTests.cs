using Shouldly;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Domain.Tests.Catalog;

public sealed class ProductTests
{
    [Fact]
    public void Create_HasOneActiveVariant_AndSearchText()
    {
        var product = TestData.Product();

        product.IsActive.ShouldBeTrue();
        product.Variants.Count.ShouldBe(1);
        product.Variants.Single().Name.ShouldBe(ProductVariant.DefaultName);
        product.SearchText.ShouldBe("kho ga la chanh brand");
    }

    [Fact]
    public void Create_WithZeroPrice_Fails()
    {
        var result = Product.Create(Guid.CreateVersion7(), TestData.Details(), Slug.FromText("x").Value, TestData.Variant(price: 0));

        result.Error.ShouldBe(CatalogErrors.InvalidPrice);
    }

    [Fact]
    public void AddVariant_WithCompareAtNotAbovePrice_Fails()
    {
        var product = TestData.Product();

        product.AddVariant(TestData.Variant("SKU-002", price: 50_000m, compareAt: 50_000m)).Error.ShouldBe(CatalogErrors.InvalidCompareAtPrice);
    }

    [Fact]
    public void AddVariant_WithDuplicateSku_Fails()
    {
        var product = TestData.Product("SKU-001");

        product.AddVariant(TestData.Variant("sku-001")).Error.ShouldBe(CatalogErrors.DuplicateSku);
    }

    [Fact]
    public void RemoveVariant_Last_Fails()
    {
        var product = TestData.Product();

        product.RemoveVariant(product.Variants.Single().Id).Error.ShouldBe(CatalogErrors.CannotRemoveLastVariant);
    }

    [Fact]
    public void UpdateVariant_DeactivatingLastActiveOfActiveProduct_Fails()
    {
        var product = TestData.Product();
        var variant = product.Variants.Single();

        product.UpdateVariant(variant.Id, TestData.Variant(), isActive: false).Error
            .ShouldBe(CatalogErrors.CannotDeactivateLastActiveVariant);
    }

    [Fact]
    public void SetVisibility_ActiveWithoutActiveVariant_Fails()
    {
        var product = TestData.Product();
        product.SetVisibility(false, false);
        product.UpdateVariant(product.Variants.Single().Id, TestData.Variant(), isActive: false).IsSuccess.ShouldBeTrue();

        product.SetVisibility(true, false).Error.ShouldBe(CatalogErrors.CannotActivateWithoutActiveVariant);
    }

    [Fact]
    public void AddImage_FirstIsPrimary_RemovingPrimaryPromotesNext()
    {
        var product = TestData.Product();
        var first = product.AddImage("/a.png", null).Value;
        var second = product.AddImage("/b.png", "alt").Value;

        first.IsPrimary.ShouldBeTrue();
        second.IsPrimary.ShouldBeFalse();

        product.RemoveImage(first.Id).Value.ShouldBe("/a.png");
        second.IsPrimary.ShouldBeTrue();
    }

    [Fact]
    public void AddImage_BeyondMax_Fails()
    {
        var product = TestData.Product();
        for (var i = 0; i < Product.MaxImages; i++)
        {
            product.AddImage($"/{i}.png", null).IsSuccess.ShouldBeTrue();
        }

        product.AddImage("/extra.png", null).Error.ShouldBe(CatalogErrors.TooManyImages);
    }

    [Fact]
    public void SetPrimaryImage_LeavesExactlyOnePrimary()
    {
        var product = TestData.Product();
        product.AddImage("/a.png", null);
        var b = product.AddImage("/b.png", null).Value;

        product.SetPrimaryImage(b.Id).IsSuccess.ShouldBeTrue();

        product.Images.Count(i => i.IsPrimary).ShouldBe(1);
        b.IsPrimary.ShouldBeTrue();
    }

    [Fact]
    public void ReorderImages_MustListEveryImageOnce()
    {
        var product = TestData.Product();
        var a = product.AddImage("/a.png", null).Value;
        var b = product.AddImage("/b.png", null).Value;

        product.ReorderImages([a.Id]).Error.ShouldBe(CatalogErrors.ImageOrderMismatch);
        product.ReorderImages([b.Id, a.Id]).IsSuccess.ShouldBeTrue();
        b.SortOrder.ShouldBe(0);
        a.SortOrder.ShouldBe(1);
    }

    [Fact]
    public void RecordSale_IncreasesSoldCount()
    {
        var product = TestData.Product();

        product.RecordSale(3);
        product.RecordSale(2);

        product.SoldCount.ShouldBe(5);
    }

    [Fact]
    public void Money_OnVariant_IsVnd()
    {
        TestData.Product().Variants.Single().Price.ShouldBe(Money.Vnd(50_000m));
    }
}
