using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SnackShop.Application.Abstractions.Services;

namespace SnackShop.Infrastructure.Email;

/// <summary>
/// In-process queue between request handlers and <see cref="EmailDispatcher"/>. Singleton.
/// Never blocks a request: if SMTP is down long enough to fill the queue, new emails are dropped and logged.
/// </summary>
public sealed partial class EmailQueue(ILogger<EmailQueue> logger) : IEmailSender
{
    public const int Capacity = 1000;

    private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(new BoundedChannelOptions(Capacity)
    {
        FullMode = BoundedChannelFullMode.DropWrite,
        SingleReader = true,
    });

    public ChannelReader<EmailMessage> Reader => _channel.Reader;

    public ValueTask QueueAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        // DropWrite reports success even when it discards the item, so check capacity first.
        if (_channel.Reader.Count >= Capacity || !_channel.Writer.TryWrite(message))
        {
            LogDropped(logger, message.Subject);
        }

        return ValueTask.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Email queue full (SMTP down?); dropped email '{Subject}'")]
    private static partial void LogDropped(ILogger logger, string subject);
}
