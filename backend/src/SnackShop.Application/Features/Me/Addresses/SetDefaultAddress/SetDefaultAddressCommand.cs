using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Me.Addresses.SetDefaultAddress;

public sealed record SetDefaultAddressCommand(Guid AddressId) : ICommand;
