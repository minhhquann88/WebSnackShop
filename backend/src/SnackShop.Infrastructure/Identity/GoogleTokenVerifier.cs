using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Domain.Common;

namespace SnackShop.Infrastructure.Identity;

public sealed class GoogleOptions
{
    public const string SectionName = "Google";

    /// <summary>OAuth client id of the web app. Empty disables Google sign-in.</summary>
    public string ClientId { get; init; } = string.Empty;
}

/// <summary>Verifies ID tokens from Google Identity Services (signature, issuer, audience, expiry) with Google.Apis.Auth.</summary>
public sealed class GoogleTokenVerifier(IOptions<GoogleOptions> options) : IGoogleTokenVerifier
{
    public static readonly Error NotConfigured = Error.Failure("Auth.GoogleNotConfigured", "Google sign-in is not configured.");
    public static readonly Error InvalidToken = Error.Unauthorized("Auth.InvalidGoogleToken", "The Google sign-in token is invalid or expired.");

    public async Task<Result<GoogleUser>> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        var clientId = options.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return Result.Failure<GoogleUser>(NotConfigured);
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId],
            });

            return new GoogleUser(payload.Subject, payload.Email, payload.EmailVerified, payload.Name, payload.Picture);
        }
        catch (InvalidJwtException)
        {
            return Result.Failure<GoogleUser>(InvalidToken);
        }
    }
}
