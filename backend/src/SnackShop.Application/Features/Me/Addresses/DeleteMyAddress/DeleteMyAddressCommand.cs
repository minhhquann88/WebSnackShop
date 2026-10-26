using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Me.Addresses.DeleteMyAddress;

public sealed record DeleteMyAddressCommand(Guid AddressId) : ICommand;
