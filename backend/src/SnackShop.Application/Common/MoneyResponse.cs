using SnackShop.Domain.Common;

namespace SnackShop.Application.Common;

/// <summary>Wire representation of <see cref="Money"/>: { "amount": 45000, "currency": "VND" }.</summary>
public sealed record MoneyResponse(decimal Amount, string Currency)
{
    public static MoneyResponse From(Money money) => new(money.Amount, money.Currency);

    public static MoneyResponse? FromNullable(Money? money) => money is null ? null : From(money);
}

/// <summary>Wire input for <see cref="Money"/>. Validators check range; handlers convert with <see cref="ToMoney"/>.</summary>
public sealed record MoneyRequest(decimal Amount, string Currency = Money.DefaultCurrency)
{
    public Money ToMoney() => Money.From(Amount, Currency);
}
