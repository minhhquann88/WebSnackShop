namespace SnackShop.Domain.Common;

/// <summary>
/// Aggregated input-validation failures keyed by property path (camelCase in the API).
/// </summary>
public sealed record ValidationError(IReadOnlyDictionary<string, string[]> Errors)
    : Error("Validation.Failed", "One or more validation errors occurred.", ErrorType.Validation)
{
    public static ValidationError FromSingle(string property, string message) =>
        new(new Dictionary<string, string[]> { [property] = [message] });
}
