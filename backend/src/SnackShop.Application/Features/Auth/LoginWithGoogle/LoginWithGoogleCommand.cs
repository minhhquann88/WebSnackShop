using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Auth.LoginWithGoogle;

/// <summary>Signs in (or signs up) a customer with a Google ID token from Google Identity Services.</summary>
public sealed record LoginWithGoogleCommand(string IdToken) : ICommand<AuthResponse>;
