using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Common;

/// <summary>
/// Builds payment instructions for unpaid bank-transfer orders from the shop's bank settings.
/// The QR image uses the public VietQR image service (img.vietqr.io), which encodes the NAPAS standard payload.
/// </summary>
public sealed class BankTransferInstructions(IApplicationDbContext db)
{
    private sealed record BankInfo(string BankName, string AccountNumber, string AccountName, string BankBin, int ExpiryHours);

    public async Task<OrderDetailResponse> AttachAsync(OrderDetailResponse order, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (!NeedsInstructions(order.PaymentMethod, order.PaymentStatus, order.Status))
        {
            return order;
        }

        var bank = await LoadAsync(cancellationToken);
        return bank is null ? order : order with
        {
            BankTransfer = Build(bank, order.OrderNumber, order.Total, order.PlacedAt),
        };
    }

    public async Task<BankTransferInstructionsResponse?> ForOrderAsync(Order order, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (!NeedsInstructions(order.PaymentMethod, order.Payment.Status, order.Status))
        {
            return null;
        }

        var bank = await LoadAsync(cancellationToken);
        return bank is null ? null : Build(bank, order.OrderNumber.Value, MoneyResponse.From(order.Total), order.PlacedAt);
    }

    public static string QrImageUrl(string bankBin, string accountNumber, string accountName, decimal amount, string content) =>
        string.Create(CultureInfo.InvariantCulture,
            $"https://img.vietqr.io/image/{bankBin}-{accountNumber}-compact2.png?amount={decimal.Round(amount):0}&addInfo={Uri.EscapeDataString(content)}&accountName={Uri.EscapeDataString(accountName)}");

    private static bool NeedsInstructions(PaymentMethod method, PaymentStatus paymentStatus, OrderStatus status) =>
        method == PaymentMethod.BankTransfer && paymentStatus == PaymentStatus.Unpaid && status == OrderStatus.Pending;

    private static BankTransferInstructionsResponse Build(BankInfo bank, string orderNumber, MoneyResponse total, DateTimeOffset placedAt)
    {
        var content = OrderNumber.FromTrusted(orderNumber).TransferContent;
        return new BankTransferInstructionsResponse(
            bank.BankName,
            bank.AccountNumber,
            bank.AccountName,
            total,
            content,
            QrImageUrl(bank.BankBin, bank.AccountNumber, bank.AccountName, total.Amount, content),
            placedAt.AddHours(bank.ExpiryHours));
    }

    private Task<BankInfo?> LoadAsync(CancellationToken cancellationToken) =>
        db.ShopSettings.AsNoTracking()
            .Select(s => new BankInfo(s.BankName, s.BankAccountNumber, s.BankAccountName, s.BankBin, s.UnpaidBankTransferExpiryHours))
            .FirstOrDefaultAsync(cancellationToken);
}
