using Shouldly;
using SnackShop.Domain.Common;
using SnackShop.Domain.Settings;

namespace SnackShop.Domain.Tests.Settings;

public sealed class ShopSettingsTests
{
    private static Result UpdateWith(ShopSettings settings, Money fee, Money? threshold, int expiryHours = 24) =>
        settings.Update(
            " Snack House ",
            "0912345678",
            "123 Lê Lợi, Q1",
            fee,
            threshold,
            "Vietcombank",
            "0011002233",
            "nguyen van a",
            "970436",
            expiryHours);

    [Fact]
    public void CreateDefault_HasSingletonId_AndSaneDefaults()
    {
        var settings = ShopSettings.CreateDefault();

        settings.Id.ShouldBe(ShopSettings.SingletonId);
        settings.FlatShippingFee.Amount.ShouldBeGreaterThan(0m);
        settings.UnpaidBankTransferExpiryHours.ShouldBe(24);
    }

    [Fact]
    public void Update_WithValidValues_TrimsAndNormalises()
    {
        var settings = ShopSettings.CreateDefault();

        var result = UpdateWith(settings, Money.Vnd(20_000m), Money.Vnd(200_000m));

        result.IsSuccess.ShouldBeTrue();
        settings.ShopName.ShouldBe("Snack House");
        settings.BankAccountName.ShouldBe("NGUYEN VAN A");
        settings.FreeShippingThreshold.ShouldBe(Money.Vnd(200_000m));
    }

    [Fact]
    public void Update_WithThresholdNotAboveFee_ReturnsTooLow()
    {
        var settings = ShopSettings.CreateDefault();

        var result = UpdateWith(settings, Money.Vnd(30_000m), Money.Vnd(30_000m));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SettingsErrors.FreeShippingThresholdTooLow);
    }

    [Fact]
    public void Update_WithZeroThreshold_ReturnsMustBePositive()
    {
        var settings = ShopSettings.CreateDefault();

        var result = UpdateWith(settings, Money.Vnd(30_000m), Money.Zero());

        result.Error.ShouldBe(SettingsErrors.FreeShippingThresholdMustBePositive);
    }

    [Fact]
    public void Update_WithNullThreshold_DisablesFreeShipping()
    {
        var settings = ShopSettings.CreateDefault();

        UpdateWith(settings, Money.Vnd(30_000m), null).IsSuccess.ShouldBeTrue();

        settings.FreeShippingThreshold.ShouldBeNull();
        settings.CalculateShippingFee(Money.Vnd(10_000_000m)).ShouldBe(Money.Vnd(30_000m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(169)]
    public void Update_WithExpiryOutOfRange_ReturnsError(int hours)
    {
        var settings = ShopSettings.CreateDefault();

        var result = UpdateWith(settings, Money.Vnd(30_000m), null, hours);

        result.Error.ShouldBe(SettingsErrors.UnpaidExpiryOutOfRange);
    }

    [Fact]
    public void CalculateShippingFee_AtOrAboveThreshold_IsFree()
    {
        var settings = ShopSettings.CreateDefault();
        UpdateWith(settings, Money.Vnd(25_000m), Money.Vnd(300_000m));

        settings.CalculateShippingFee(Money.Vnd(299_999m)).ShouldBe(Money.Vnd(25_000m));
        settings.CalculateShippingFee(Money.Vnd(300_000m)).ShouldBe(Money.Zero());
        settings.CalculateShippingFee(Money.Vnd(500_000m)).ShouldBe(Money.Zero());
    }
}
