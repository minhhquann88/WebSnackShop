using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common.Email;

namespace SnackShop.Application.Features.Auth.Common;

/// <summary>Account-related emails (confirmation, password reset).</summary>
public sealed class AccountEmails(IEmailSender emailSender, IAppLinks links, IIdentityService identity)
{
    public async Task SendConfirmationAsync(UserAccount account, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(account);

        var token = await identity.GenerateEmailConfirmationTokenAsync(account.Id, cancellationToken);
        var link = links.Frontend($"xac-nhan-email?userId={account.Id}&token={Uri.EscapeDataString(token)}");
        var body = EmailTemplates.Paragraph($"Xin chào {account.FullName},")
            + EmailTemplates.Paragraph("Cảm ơn bạn đã đăng ký. Bấm nút bên dưới để xác nhận địa chỉ email.")
            + EmailTemplates.Button("Xác nhận email", link);

        await emailSender.QueueAsync(new EmailMessage(account.Email, "Xác nhận email của bạn",
            EmailTemplates.Layout(links.ShopName, "Xác nhận email", body)), cancellationToken);
    }

    public async Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken)
    {
        var link = links.Frontend($"dat-lai-mat-khau?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}");
        var body = EmailTemplates.Paragraph("Bạn (hoặc ai đó) vừa yêu cầu đặt lại mật khẩu. Liên kết có hiệu lực trong 1 giờ.")
            + EmailTemplates.Button("Đặt lại mật khẩu", link)
            + EmailTemplates.Paragraph("Nếu không phải bạn, hãy bỏ qua email này.");

        await emailSender.QueueAsync(new EmailMessage(email, "Đặt lại mật khẩu",
            EmailTemplates.Layout(links.ShopName, "Đặt lại mật khẩu", body)), cancellationToken);
    }
}
