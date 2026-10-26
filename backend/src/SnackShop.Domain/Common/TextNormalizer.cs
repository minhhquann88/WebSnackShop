using System.Globalization;
using System.Text;

namespace SnackShop.Domain.Common;

/// <summary>
/// Vietnamese-aware text folding: lowercases and strips diacritics ("Khô Gà Lá Chanh" → "kho ga la chanh").
/// Used for slugs and for the denormalised search column, so search works without Postgres extensions.
/// </summary>
public static class TextNormalizer
{
    public static string RemoveDiacritics(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var decomposed = text.Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Lowercase, no diacritics, single spaces. Apply to both stored search text and user search terms.</summary>
    public static string ForSearch(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var folded = RemoveDiacritics(text).ToLowerInvariant();
        var builder = new StringBuilder(folded.Length);
        var lastWasSpace = true;

        foreach (var c in folded)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
                lastWasSpace = false;
            }
            else if (!lastWasSpace)
            {
                builder.Append(' ');
                lastWasSpace = true;
            }
        }

        return builder.ToString().TrimEnd();
    }
}
