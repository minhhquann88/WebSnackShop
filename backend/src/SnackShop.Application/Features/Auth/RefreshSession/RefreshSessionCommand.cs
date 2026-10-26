using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Auth.RefreshSession;

/// <summary>Exchanges a refresh token for a new token pair (the old refresh token is revoked).</summary>
public sealed record RefreshSessionCommand(string RefreshToken) : ICommand<AuthResponse>;
