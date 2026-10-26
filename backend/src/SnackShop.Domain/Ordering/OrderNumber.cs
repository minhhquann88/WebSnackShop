using System.Globalization;
using SnackShop.Domain.Common;

namespace SnackShop.Domain.Ordering;

/// <summary>Human-readable order reference: SS-yyyyMMdd-NNNNN (date in Vietnam time, NNNNN from a DB sequence).</summary>
public sealed record OrderNumber
{
    public const int MaxLength = 20;
    private const string Prefix = "SS";

    private OrderNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>Bank transfer description, alphanumeric only because banks strip punctuation: SS2026102200042.</summary>
    public string TransferContent => Value.Replace("-", string.Empty, StringComparison.Ordinal);

    public static OrderNumber Create(DateTimeOffset placedAt, long sequence)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequence);

        var date = ShopTime.ToVietnam(placedAt).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var number = (sequence % 100_000).ToString("D5", CultureInfo.InvariantCulture);
        return new OrderNumber($"{Prefix}-{date}-{number}");
    }

    /// <summary>Rehydrates a stored value (persistence and lookups by number).</summary>
    public static OrderNumber FromTrusted(string value) => new(value.Trim().ToUpperInvariant());

    public override string ToString() => Value;
}
