using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Me.UpdateProfile;

public sealed record UpdateProfileCommand(string FullName, string? PhoneNumber) : ICommand;
