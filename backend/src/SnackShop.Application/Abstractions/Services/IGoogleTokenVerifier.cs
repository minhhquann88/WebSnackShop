using SnackShop.Domain.Common;

namespace SnackShop.Application.Abstractions.Services;

public sealed record GoogleUser(string Subject, string Email, bool EmailVerified, string? Name, string? Picture);

/// <summary>Validates a Google Identity Services ID token (signature, audience, expiry).</summary>
public interface IGoogleTokenVerifier
{
    Task<Result<GoogleUser>> VerifyAsync(string idToken, CancellationToken cancellationToken);
}
