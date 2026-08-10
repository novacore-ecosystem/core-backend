using System.Net;
using System.Text;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Builders.HtmlComponents;

internal static class KeyValueSection
{
    public static string Render(IReadOnlyDictionary<string, string> items)
    {
        var builder = new StringBuilder();
        builder.Append($"""<table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="margin:0 0 16px;border-collapse:collapse;font-family:{HtmlStyles.FontFamily};font-size:14px;">""");

        foreach (var (key, value) in items)
        {
            builder.Append($"""
                <tr>
                  <td style="padding:6px 12px 6px 0;color:{HtmlStyles.MutedTextColor};vertical-align:top;white-space:nowrap;">
                    {WebUtility.HtmlEncode(key)}
                  </td>
                  <td style="padding:6px 0;color:{HtmlStyles.TextColor};font-weight:600;">
                    {WebUtility.HtmlEncode(value)}
                  </td>
                </tr>
                """);
        }

        builder.Append("</table>");

        return builder.ToString();
    }
}
