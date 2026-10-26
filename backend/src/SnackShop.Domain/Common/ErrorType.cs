namespace SnackShop.Domain.Common;

/// <summary>
/// Category of a business error. The API layer maps each type to an HTTP status code
/// (see docs/05-api-guidelines.md).
/// </summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
}
