using SnackShop.Domain.Common;

namespace SnackShop.Domain.Ordering;

/// <summary>Payment record of an order (one per order at MVP). Shaped to accept online gateways later (docs/adr/0005).</summary>
public sealed class Payment : Entity
{
    public const int MaxProviderLength = 30;
    public const int MaxTransactionRefLength = 100;
    public const int MaxTransferContentLength = 50;

    private Payment()
    {
    }

    internal Payment(Guid orderId, PaymentMethod method, Money amount, string? transferContent)
        : base(Guid.CreateVersion7())
    {
        OrderId = orderId;
        Method = method;
        Status = PaymentStatus.Unpaid;
        Amount = amount;
        Provider = method == PaymentMethod.Cod ? "COD" : "BankTransfer";
        TransferContent = transferContent;
    }

    public Guid OrderId { get; private set; }

    public PaymentMethod Method { get; private set; }

    public PaymentStatus Status { get; private set; }

    public Money Amount { get; private set; } = default!;

    public string Provider { get; private set; } = default!;

    public string? TransactionRef { get; private set; }

    public string? TransferContent { get; private set; }

    public DateTimeOffset? PaidAt { get; private set; }

    public Guid? ConfirmedByUserId { get; private set; }

    public bool IsPaid => Status == PaymentStatus.Paid;

    internal void MarkPaid(string? transactionRef, DateTimeOffset paidAt, Guid? confirmedBy)
    {
        Status = PaymentStatus.Paid;
        TransactionRef = string.IsNullOrWhiteSpace(transactionRef) ? TransactionRef : transactionRef.Trim();
        PaidAt = paidAt;
        ConfirmedByUserId = confirmedBy;
    }

    internal void MarkRefundPending() => Status = PaymentStatus.RefundPending;

    internal void MarkRefunded(string? transactionRef)
    {
        Status = PaymentStatus.Refunded;
        if (!string.IsNullOrWhiteSpace(transactionRef))
        {
            TransactionRef = transactionRef.Trim();
        }
    }
}
