using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthResponse>;
