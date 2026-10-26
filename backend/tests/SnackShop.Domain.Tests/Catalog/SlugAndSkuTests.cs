using Shouldly;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Domain.Tests.Catalog;

public sealed class SlugAndSkuTests
{
    [Theory]
    [InlineData("Khô Gà Lá Chanh 200g", "kho-ga-la-chanh-200g")]
    [InlineData("Bánh tráng trộn – Tây Ninh!!", "banh-trang-tron-tay-ninh")]
    [InlineData("  Đậu phộng   rang  ", "dau-phong-rang")]
    [InlineData("Mứt dừa (đặc biệt)", "mut-dua-dac-biet")]
    public void FromText_FoldsVietnameseIntoUrlSafeSlug(string input, string expected)
    {
        Slug.FromText(input).Value.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    public void FromText_WithNoLettersOrDigits_Fails(string input)
    {
        Slug.FromText(input).Error.ShouldBe(CatalogErrors.InvalidSlug);
    }

    [Theory]
    [InlineData("Has-Upper")]
    [InlineData("double--hyphen")]
    [InlineData("-leading")]
    [InlineData("có-dấu")]
    public void Create_RejectsInvalidSlugs(string value)
    {
        Slug.Create(value).IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void FromText_TruncatesTo120Characters()
    {
        var slug = Slug.FromText(new string('a', 200)).Value;

        slug.Value.Length.ShouldBe(Slug.MaxLength);
    }

    [Theory]
    [InlineData("ss-0001", "SS-0001")]
    [InlineData(" kg200 ", "KG200")]
    public void Sku_Create_NormalisesToUppercase(string input, string expected)
    {
        Sku.Create(input).Value.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("SKU 001")]
    [InlineData("SKU_001")]
    public void Sku_Create_RejectsInvalid(string input)
    {
        Sku.Create(input).Error.ShouldBe(CatalogErrors.InvalidSku);
    }

    [Theory]
    [InlineData("Hạt Điều Bình Phước", "hat dieu binh phuoc")]
    [InlineData("KHÔ-BÒ, miếng!", "kho bo mieng")]
    public void TextNormalizer_ForSearch_IsAccentAndCaseInsensitive(string input, string expected)
    {
        TextNormalizer.ForSearch(input).ShouldBe(expected);
    }
}
