using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Me.Addresses.AddMyAddress;

public sealed record AddMyAddressCommand(string? Label, AddressRequest Address, bool IsDefault) : ICommand<Guid>;
