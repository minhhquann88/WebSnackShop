using System.Text.RegularExpressions;
using SnackShop.Domain.Common;

namespace SnackShop.Domain.Catalog;

/// <summary>Stock keeping unit, unique across the catalog. Uppercase letters, digits and hyphens, 3–50 characters.</summary>
public sealed partial record Sku
{
    public const int MaxLength = 50;

    private Sku(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Sku> Create(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToUpperInvariant();
        if (!SkuPattern().IsMatch(normalized))
        {
            return Result.Failure<Sku>(CatalogErrors.InvalidSku);
        }

        return new Sku(normalized);
    }

    /// <summary>Rehydrates a value already validated (persistence only).</summary>
    public static Sku FromTrusted(string value) => new(value);

    public override string ToString() => Value;

    [GeneratedRegex("^[A-Z0-9-]{3,50}$", RegexOptions.CultureInvariant)]
    private static partial Regex SkuPattern();
}
