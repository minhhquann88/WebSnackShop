namespace SnackShop.Application.Features.Me.Addresses;

public sealed record SavedAddressResponse(
    Guid Id,
    string? Label,
    string FullName,
    string Phone,
    string Line1,
    string Ward,
    string District,
    string Province,
    bool IsDefault);
