using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Me.Addresses.UpdateMyAddress;

public sealed record UpdateMyAddressCommand(Guid AddressId, string? Label, AddressRequest Address) : ICommand;
