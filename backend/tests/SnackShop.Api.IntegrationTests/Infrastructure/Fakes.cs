using System.Collections.Concurrent;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Domain.Common;

namespace SnackShop.Api.IntegrationTests.Infrastructure;

/// <summary>Captures emails instead of sending them.</summary>
public sealed class FakeEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<EmailMessage> _sent = new();

    public IReadOnlyCollection<EmailMessage> Sent => _sent.ToArray();

    public ValueTask QueueAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        _sent.Enqueue(message);
        return ValueTask.CompletedTask;
    }

    public void Clear() => _sent.Clear();
}

/// <summary>Accepts ID tokens of the form "google:{subject}:{email}[:unverified]".</summary>
public sealed class FakeGoogleTokenVerifier : IGoogleTokenVerifier
{
    public static readonly Error Invalid = Error.Unauthorized("Auth.InvalidGoogleToken", "Invalid token.");

    public Task<Result<GoogleUser>> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        var parts = idToken.Split(':');
        if (parts.Length < 3 || parts[0] != "google")
        {
            return Task.FromResult(Result.Failure<GoogleUser>(Invalid));
        }

        var verified = parts.Length < 4 || parts[3] != "unverified";
        return Task.FromResult(Result.Success(new GoogleUser(parts[1], parts[2], verified, "Google User", null)));
    }

    public static string Token(string subject, string email, bool verified = true) =>
        $"google:{subject}:{email}" + (verified ? string.Empty : ":unverified");
}
