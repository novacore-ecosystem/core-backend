using System.Net;
using System.Text;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Builders.HtmlComponents;

internal static class DataTable
{
    public static string Render(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var builder = new StringBuilder();
        builder.Append($"""<table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="margin:0 0 16px;border-collapse:collapse;font-family:{HtmlStyles.FontFamily};font-size:14px;">""");

        builder.Append("<tr>");
        foreach (var header in headers)
        {
            builder.Append($"""<th style="text-align:left;padding:8px 12px;background-color:{HtmlStyles.TableHeaderBackground};border-bottom:2px solid {HtmlStyles.BorderColor};color:{HtmlStyles.TextColor};font-weight:600;">{WebUtility.HtmlEncode(header)}</th>""");
        }

        builder.Append("</tr>");

        foreach (var row in rows)
        {
            builder.Append("<tr>");
            foreach (var cell in row)
                builder.Append($"""<td style="padding:8px 12px;border-bottom:1px solid {HtmlStyles.BorderColor};color:{HtmlStyles.TextColor};">{WebUtility.HtmlEncode(cell)}</td>""");
            builder.Append("</tr>");
        }

        builder.Append("</table>");

        return builder.ToString();
    }
}
