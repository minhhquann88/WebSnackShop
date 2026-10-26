namespace SnackShop.Domain.Customers;

/// <summary>Postal fields shared by saved addresses and order shipping addresses. Validated by the application layer.</summary>
public sealed record AddressInfo(
    string FullName,
    string Phone,
    string Line1,
    string Ward,
    string District,
    string Province)
{
    public const int MaxNameLength = 100;
    public const int MaxPhoneLength = 15;
    public const int MaxLineLength = 250;
    public const int MaxAreaLength = 100;

    /// <summary>Vietnamese mobile/landline: 10 digits starting with 0.</summary>
    public const string PhonePattern = @"^0\d{9}$";

    public AddressInfo Normalize() => new(
        FullName.Trim(),
        Phone.Trim(),
        Line1.Trim(),
        Ward.Trim(),
        District.Trim(),
        Province.Trim());
}
