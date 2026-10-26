namespace SnackShop.Domain.Common;

/// <summary>
/// An amount in a currency. Stored as numeric(18,2) + char(3). VND has no fractional unit in practice,
/// but the precision is kept so adding another currency later needs no schema change.
/// </summary>
public sealed record Money
{
    public const string DefaultCurrency = "VND";

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Money Zero(string currency = DefaultCurrency) => new(0m, currency);

    public static Money Vnd(decimal amount) => From(amount, DefaultCurrency);

    /// <summary>Creates a non-negative amount. Throws on negative: negative money is a programming error, not a business one.</summary>
    public static Money From(decimal amount, string currency = DefaultCurrency)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (currency.Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));
        }

        return new Money(decimal.Round(amount, 2, MidpointRounding.ToEven), currency.ToUpperInvariant());
    }

    public bool IsZero => Amount == 0m;

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        var result = left.Amount - right.Amount;
        if (result < 0m)
        {
            throw new InvalidOperationException("Money subtraction cannot produce a negative amount.");
        }

        return new Money(result, left.Currency);
    }

    public static Money operator *(Money money, int factor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(factor);
        return new Money(money.Amount * factor, money.Currency);
    }

    public static bool operator >(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right) => !(left < right);

    public static bool operator <=(Money left, Money right) => !(left > right);

    public static Money Add(Money left, Money right) => left + right;

    public static Money Subtract(Money left, Money right) => left - right;

    public static Money Multiply(Money money, int factor) => money * factor;

    public int CompareTo(Money other)
    {
        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    public override string ToString() => $"{Amount:0.##} {Currency}";

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Cannot operate on different currencies: {left.Currency} and {right.Currency}.");
        }
    }
}
