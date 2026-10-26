using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Auth.Logout;

/// <summary>Revokes the given refresh token. Always succeeds (unknown tokens are ignored).</summary>
public sealed record LogoutCommand(string RefreshToken) : ICommand;
