using System.Net;
using System.Text;

using NovaCore.BuildingBlock.Infrastructure.Mail.Builders.HtmlComponents;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Builders;

public sealed class EmailBodyBuilder
{
    private readonly StringBuilder _content = new();

    private EmailBodyBuilder()
    {
    }

    public static EmailBodyBuilder Create() => new();

    public EmailBodyBuilder Heading(string text, int level = 2)
    {
        var htmlLevel = Math.Clamp(level, 1, 4);
        var fontSize = htmlLevel switch
        {
            1 => "28px",
            2 => "22px",
            3 => "18px",
            _ => "16px",
        };

        _content.Append($"""<h{htmlLevel} style="margin:0 0 16px;font-family:{HtmlStyles.FontFamily};font-size:{fontSize};font-weight:600;color:{HtmlStyles.TextColor};line-height:1.3;">{WebUtility.HtmlEncode(text)}</h{htmlLevel}>""");

        return this;
    }

    public EmailBodyBuilder Paragraph(string text)
    {
        _content.Append($"""<p style="margin:0 0 16px;font-family:{HtmlStyles.FontFamily};font-size:15px;color:{HtmlStyles.TextColor};line-height:1.6;">{WebUtility.HtmlEncode(text)}</p>""");

        return this;
    }

    public EmailBodyBuilder Text(string text) => Paragraph(text);

    public EmailBodyBuilder SmallText(string text)
    {
        _content.Append($"""<p style="margin:0 0 12px;font-family:{HtmlStyles.FontFamily};font-size:12px;color:{HtmlStyles.MutedTextColor};line-height:1.5;">{WebUtility.HtmlEncode(text)}</p>""");

        return this;
    }

    public EmailBodyBuilder Divider()
    {
        _content.Append($"""<hr style="border:none;border-top:1px solid {HtmlStyles.BorderColor};margin:24px 0;" />""");

        return this;
    }

    public EmailBodyBuilder Button(string label, string url)
    {
        _content.Append(HtmlComponents.Button.Render(label, url));

        return this;
    }

    public EmailBodyBuilder Hyperlink(string label, string url)
    {
        var encodedUrl = WebUtility.HtmlEncode(url);
        _content.Append($"""<p style="margin:0 0 16px;font-family:{HtmlStyles.FontFamily};font-size:15px;line-height:1.6;"><a href="{encodedUrl}" style="color:{HtmlStyles.PrimaryColor};text-decoration:underline;">{WebUtility.HtmlEncode(label)}</a></p>""");

        return this;
    }

    public EmailBodyBuilder Image(string url, string altText, int? width = null)
    {
        var widthAttribute = width.HasValue ? $" width=\"{width}\"" : string.Empty;
        _content.Append($"""<img src="{WebUtility.HtmlEncode(url)}" alt="{WebUtility.HtmlEncode(altText)}"{widthAttribute} style="max-width:100%;display:block;margin:0 0 16px;border:0;" />""");

        return this;
    }

    public EmailBodyBuilder Table(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        _content.Append(HtmlComponents.DataTable.Render(headers, rows));

        return this;
    }

    public EmailBodyBuilder KeyValueSection(IReadOnlyDictionary<string, string> items)
    {
        _content.Append(HtmlComponents.KeyValueSection.Render(items));

        return this;
    }

    public EmailBodyBuilder AlertBox(string message) =>
        AppendCallout(message, HtmlStyles.AlertBackground, HtmlStyles.AlertBorder, HtmlStyles.AlertText);

    public EmailBodyBuilder SuccessBox(string message) =>
        AppendCallout(message, HtmlStyles.SuccessBackground, HtmlStyles.SuccessBorder, HtmlStyles.SuccessText);

    public EmailBodyBuilder WarningBox(string message) =>
        AppendCallout(message, HtmlStyles.WarningBackground, HtmlStyles.WarningBorder, HtmlStyles.WarningText);

    public EmailBodyBuilder CodeBlock(string code)
    {
        _content.Append($"""<pre style="margin:0 0 16px;padding:12px 16px;background-color:{HtmlStyles.CodeBackground};border-radius:4px;font-family:'Courier New',monospace;font-size:13px;color:{HtmlStyles.TextColor};overflow-x:auto;white-space:pre-wrap;word-break:break-word;">{WebUtility.HtmlEncode(code)}</pre>""");

        return this;
    }

    public EmailBodyBuilder Spacer(int height = 24)
    {
        _content.Append($"""<div style="height:{height}px;line-height:{height}px;font-size:1px;">&nbsp;</div>""");

        return this;
    }

    public EmailBodyBuilder Footer(string text)
    {
        _content.Append($"""<p style="margin:24px 0 0;font-family:{HtmlStyles.FontFamily};font-size:12px;color:{HtmlStyles.MutedTextColor};line-height:1.5;text-align:center;">{WebUtility.HtmlEncode(text)}</p>""");

        return this;
    }

    public string Build() => _content.ToString();

    private EmailBodyBuilder AppendCallout(string message, string background, string border, string textColor)
    {
        _content.Append(HtmlComponents.Callout.Render(message, background, border, textColor));

        return this;
    }
}
