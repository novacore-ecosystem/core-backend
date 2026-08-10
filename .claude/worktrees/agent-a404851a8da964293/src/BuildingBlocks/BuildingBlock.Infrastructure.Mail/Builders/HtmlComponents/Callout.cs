using System.Net;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Builders.HtmlComponents;

internal static class Callout
{
    public static string Render(string message, string background, string border, string textColor)
    {
        return $"""
            <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="margin:0 0 16px;">
              <tr>
                <td style="padding:12px 16px;background-color:{background};border:1px solid {border};border-radius:6px;font-family:{HtmlStyles.FontFamily};font-size:14px;color:{textColor};line-height:1.5;">{WebUtility.HtmlEncode(message)}</td>
              </tr>
            </table>
            """;
    }
}
