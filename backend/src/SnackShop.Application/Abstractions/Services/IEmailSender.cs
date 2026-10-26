namespace SnackShop.Application.Abstractions.Services;

public sealed record EmailMessage(string To, string Subject, string HtmlBody);

/// <summary>
/// Queues an email for background delivery. Never blocks the request on SMTP and never throws for delivery
/// problems; failures are retried and logged by the dispatcher.
/// </summary>
public interface IEmailSender
{
    ValueTask QueueAsync(EmailMessage message, CancellationToken cancellationToken);
}
