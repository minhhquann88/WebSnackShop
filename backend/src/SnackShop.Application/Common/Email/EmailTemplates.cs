using System.Globalization;
using System.Net;
using System.Text;

namespace SnackShop.Application.Common.Email;

/// <summary>
/// Minimal, inline-styled HTML emails in Vietnamese. Every dynamic value is HTML-encoded.
/// Kept in code (not files) so they are type-checked and versioned with the features using them.
/// </summary>
public static class EmailTemplates
{
    private static readonly CultureInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN");

    public static string Layout(string shopName, string title, string bodyHtml) => $"""
        <!doctype html>
        <html lang="vi"><body style="margin:0;padding:24px;background:#f6f6f6;font-family:Arial,Helvetica,sans-serif;color:#222">
          <div style="max-width:560px;margin:0 auto;background:#fff;border-radius:8px;padding:24px">
            <h2 style="margin:0 0 16px;color:#d9480f">{Encode(shopName)}</h2>
            <h3 style="margin:0 0 16px">{Encode(title)}</h3>
            {bodyHtml}
            <p style="margin-top:32px;font-size:12px;color:#888">Email này được gửi tự động, vui lòng không trả lời.</p>
          </div>
        </body></html>
        """;

    public static string Paragraph(string text) => $"<p style=\"line-height:1.5\">{Encode(text)}</p>";

    public static string Button(string text, string url) =>
        $"<p><a href=\"{Encode(url)}\" style=\"display:inline-block;background:#d9480f;color:#fff;padding:12px 20px;border-radius:6px;text-decoration:none\">{Encode(text)}</a></p>";

    public static string Money(decimal amount) => amount.ToString("#,##0", Vietnamese) + " ₫";

    public static string Table(IEnumerable<(string Left, string Right)> rows)
    {
        var sb = new StringBuilder("<table style=\"width:100%;border-collapse:collapse\">");
        foreach (var (left, right) in rows)
        {
            sb.Append("<tr><td style=\"padding:6px 0;border-bottom:1px solid #eee\">").Append(Encode(left))
              .Append("</td><td style=\"padding:6px 0;border-bottom:1px solid #eee;text-align:right\">").Append(Encode(right))
              .Append("</td></tr>");
        }

        return sb.Append("</table>").ToString();
    }

    public static string Image(string url, string alt) =>
        $"<p style=\"text-align:center\"><img src=\"{Encode(url)}\" alt=\"{Encode(alt)}\" width=\"260\" style=\"max-width:100%\"/></p>";

    public static string Encode(string value) => WebUtility.HtmlEncode(value);
}
