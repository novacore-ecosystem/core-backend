using System.Net;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Builders.HtmlComponents;

internal static class Button
{
    public static string Render(string label, string url)
    {
        var encodedLabel = WebUtility.HtmlEncode(label);
        var encodedUrl = WebUtility.HtmlEncode(url);

        return $"""
            <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="margin:0 0 16px;">
              <tr>
                <td style="border-radius:6px;background-color:{HtmlStyles.PrimaryColor};">
                  <a href="{encodedUrl}" target="_blank" rel="noopener" style="display:inline-block;padding:12px 24px;font-family:{HtmlStyles.FontFamily};font-size:15px;font-weight:600;color:{HtmlStyles.PrimaryTextColor};text-decoration:none;border-radius:6px;">{encodedLabel}</a>
                </td>
              </tr>
            </table>
            """;
    }
}
