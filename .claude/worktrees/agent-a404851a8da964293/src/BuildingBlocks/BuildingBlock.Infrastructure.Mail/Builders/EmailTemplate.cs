using System.Net;

using NovaCore.BuildingBlock.Infrastructure.Mail.Builders.HtmlComponents;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Builders;

public sealed class EmailTemplate
{
    public static readonly EmailTemplate Default = new(null, null, null);

    private readonly string? _companyName;
    private readonly string? _logoUrl;
    private readonly string? _footerText;

    private EmailTemplate(string? companyName, string? logoUrl, string? footerText)
    {
        _companyName = companyName;
        _logoUrl = logoUrl;
        _footerText = footerText;
    }

    public EmailTemplate WithCompany(string companyName, string? logoUrl = null) =>
        new(companyName, logoUrl, _footerText);

    public EmailTemplate WithFooter(string footerText) =>
        new(_companyName, _logoUrl, footerText);

    public string Wrap(string bodyContent)
    {
        var header = BuildHeader();
        var footer = BuildFooter();
        var title = WebUtility.HtmlEncode(_companyName ?? string.Empty);

        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>{title}</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f3f4f6;font-family:{HtmlStyles.FontFamily};">
              <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="background-color:#f3f4f6;padding:24px 0;">
                <tr>
                  <td align="center">
                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="600" style="max-width:600px;width:100%;background-color:#ffffff;border-radius:8px;overflow:hidden;">
                      {header}
                      <tr>
                        <td style="padding:32px;">
                          {bodyContent}
                        </td>
                      </tr>
                      {footer}
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    private string BuildHeader()
    {
        if (string.IsNullOrWhiteSpace(_logoUrl) && string.IsNullOrWhiteSpace(_companyName))
            return string.Empty;

        var content = !string.IsNullOrWhiteSpace(_logoUrl)
            ? $"""<img src="{WebUtility.HtmlEncode(_logoUrl)}" alt="{WebUtility.HtmlEncode(_companyName ?? string.Empty)}" style="max-height:40px;display:block;" />"""
            : $"""<span style="font-family:{HtmlStyles.FontFamily};font-size:20px;font-weight:700;color:{HtmlStyles.TextColor};">{WebUtility.HtmlEncode(_companyName)}</span>""";

        return $"""
            <tr>
              <td style="padding:24px 32px;border-bottom:1px solid {HtmlStyles.BorderColor};">
                {content}
              </td>
            </tr>
            """;
    }

    private string BuildFooter()
    {
        var footerText = _footerText ?? "This is an automated message, please do not reply.";
        var companyLine = !string.IsNullOrWhiteSpace(_companyName)
            ? $"&copy; {DateTime.UtcNow.Year} {WebUtility.HtmlEncode(_companyName)}. All rights reserved."
            : $"&copy; {DateTime.UtcNow.Year}. All rights reserved.";

        return $"""
            <tr>
              <td style="padding:24px 32px;background-color:{HtmlStyles.TableHeaderBackground};border-top:1px solid {HtmlStyles.BorderColor};text-align:center;">
                <p style="margin:0 0 4px;font-family:{HtmlStyles.FontFamily};font-size:12px;color:{HtmlStyles.MutedTextColor};">{WebUtility.HtmlEncode(footerText)}</p>
                <p style="margin:0;font-family:{HtmlStyles.FontFamily};font-size:12px;color:{HtmlStyles.MutedTextColor};">{companyLine}</p>
              </td>
            </tr>
            """;
    }
}
