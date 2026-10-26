using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SnackShop.Application.Abstractions.Services;

namespace SnackShop.Infrastructure.Email;

/// <summary>
/// Sends queued emails over SMTP with up to three attempts. Delivery failures are logged, never thrown to callers.
/// Emails still in memory are lost on process restart (accepted at MVP; outbox planned, docs/02 §7).
/// </summary>
public sealed partial class EmailDispatcher(EmailQueue queue, IOptions<EmailOptions> options, ILogger<EmailDispatcher> logger)
    : BackgroundService
{
    private const int MaxAttempts = 3;
    private readonly EmailOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.Reader.ReadAllAsync(stoppingToken))
        {
            await SendWithRetryAsync(message, stoppingToken);
        }
    }

    private async Task SendWithRetryAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var maskedTo = MaskEmail(message.To);

        if (!_options.Enabled)
        {
            LogSkipped(logger, maskedTo, message.Subject);
            return;
        }

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await SendAsync(message, cancellationToken);
                LogSent(logger, maskedTo, message.Subject);
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogFailed(logger, ex, maskedTo, attempt);
                if (attempt < MaxAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 5), cancellationToken);
                }
            }
        }
    }

    private async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        using var mime = new MimeMessage();
        mime.From.Add(MailboxAddress.Parse(_options.From));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        var security = _options.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
        await client.ConnectAsync(_options.Host, _options.Port, security, cancellationToken);

        if (!string.IsNullOrEmpty(_options.Username))
        {
            await client.AuthenticateAsync(_options.Username, _options.Password ?? string.Empty, cancellationToken);
        }

        await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }

    /// <summary>"nguyenvana@gmail.com" → "ng***@gmail.com" (docs/04 §8: no full emails in logs).</summary>
    internal static string MaskEmail(string email)
    {
        var at = email.IndexOf('@', StringComparison.Ordinal);
        return at <= 2 ? "***" + email[Math.Max(at, 0)..] : email[..2] + "***" + email[at..];
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Email '{Subject}' sent to {To}")]
    private static partial void LogSent(ILogger logger, string to, string subject);

    [LoggerMessage(Level = LogLevel.Information, Message = "Email disabled; skipped '{Subject}' to {To}")]
    private static partial void LogSkipped(ILogger logger, string to, string subject);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sending email to {To} failed (attempt {Attempt})")]
    private static partial void LogFailed(ILogger logger, Exception exception, string to, int attempt);
}
