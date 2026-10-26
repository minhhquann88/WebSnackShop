using Shouldly;
using SnackShop.Domain.Common;

namespace SnackShop.Domain.Tests.Common;

public sealed class MoneyTests
{
    [Fact]
    public void From_RoundsToTwoDecimals_AndUppercasesCurrency()
    {
        var money = Money.From(10.005m, "vnd");

        money.Amount.ShouldBe(10.00m);
        money.Currency.ShouldBe("VND");
    }

    [Fact]
    public void From_NegativeAmount_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Money.From(-1m));
    }

    [Theory]
    [InlineData("")]
    [InlineData("VN")]
    [InlineData("VNDD")]
    public void From_InvalidCurrency_Throws(string currency)
    {
        Should.Throw<ArgumentException>(() => Money.From(1m, currency));
    }

    [Fact]
    public void Add_SameCurrency_Sums()
    {
        (Money.Vnd(10_000m) + Money.Vnd(5_000m)).ShouldBe(Money.Vnd(15_000m));
    }

    [Fact]
    public void Add_DifferentCurrency_Throws()
    {
        Should.Throw<InvalidOperationException>(() => Money.Vnd(1m) + Money.From(1m, "USD"));
    }

    [Fact]
    public void Subtract_BelowZero_Throws()
    {
        Should.Throw<InvalidOperationException>(() => Money.Vnd(1m) - Money.Vnd(2m));
    }

    [Fact]
    public void Multiply_ByQuantity()
    {
        (Money.Vnd(45_000m) * 3).ShouldBe(Money.Vnd(135_000m));
    }

    [Fact]
    public void Comparison_Works()
    {
        (Money.Vnd(2m) > Money.Vnd(1m)).ShouldBeTrue();
        (Money.Vnd(1m) >= Money.Vnd(1m)).ShouldBeTrue();
        (Money.Vnd(1m) < Money.Vnd(2m)).ShouldBeTrue();
    }

    [Fact]
    public void Equality_IsStructural()
    {
        Money.Vnd(100m).ShouldBe(Money.From(100m, "VND"));
        Money.Zero().IsZero.ShouldBeTrue();
    }
}
