using System.Text.RegularExpressions;
using SnackShop.Domain.Common;

namespace SnackShop.Domain.Catalog;

/// <summary>URL-safe identifier: lowercase ASCII letters, digits and single hyphens, at most 120 characters.</summary>
public sealed partial record Slug
{
    public const int MaxLength = 120;

    private Slug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>Builds a slug from free text, e.g. "Khô gà lá chanh 200g" → "kho-ga-la-chanh-200g".</summary>
    public static Result<Slug> FromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<Slug>(CatalogErrors.InvalidSlug);
        }

        var value = TextNormalizer.ForSearch(text).Replace(' ', '-');
        if (value.Length > MaxLength)
        {
            value = value[..MaxLength].TrimEnd('-');
        }

        return Create(value);
    }

    /// <summary>Validates an explicit slug supplied by an admin.</summary>
    public static Result<Slug> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaxLength || !SlugPattern().IsMatch(value))
        {
            return Result.Failure<Slug>(CatalogErrors.InvalidSlug);
        }

        return new Slug(value);
    }

    /// <summary>Rehydrates a value already validated (persistence only).</summary>
    public static Slug FromTrusted(string value) => new(value);

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();
}
