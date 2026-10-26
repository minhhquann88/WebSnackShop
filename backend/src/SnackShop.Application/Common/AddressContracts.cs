using SnackShop.Domain.Customers;

namespace SnackShop.Application.Common;

/// <summary>Wire input for a postal address (checkout and address book).</summary>
public sealed record AddressRequest(
    string FullName,
    string Phone,
    string Line1,
    string Ward,
    string District,
    string Province)
{
    public AddressInfo ToInfo() => new AddressInfo(FullName, Phone, Line1, Ward, District, Province).Normalize();
}

/// <summary>Wire representation of a postal address.</summary>
public sealed record AddressResponse(
    string FullName,
    string Phone,
    string Line1,
    string Ward,
    string District,
    string Province)
{
    public static AddressResponse From(AddressInfo info) =>
        new(info.FullName, info.Phone, info.Line1, info.Ward, info.District, info.Province);
}
