using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Me.Addresses.ListMyAddresses;

/// <summary>Address book of the signed-in customer, default first. Bounded (max 5), so not paged.</summary>
public sealed record ListMyAddressesQuery : IQuery<IReadOnlyList<SavedAddressResponse>>;
