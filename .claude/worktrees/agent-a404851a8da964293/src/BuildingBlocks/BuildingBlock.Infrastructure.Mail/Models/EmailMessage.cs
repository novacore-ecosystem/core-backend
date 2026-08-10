namespace NovaCore.BuildingBlock.Infrastructure.Mail.Models;

public sealed record EmailMessage
{
    public required string Subject { get; init; }
    public string? HtmlBody { get; init; }
    public string? TextBody { get; init; }
    /// <summary>Overrides the default sender configured in <c>MailOptions</c> when provided.</summary>
    public EmailAddress? From { get; init; }
    public required IReadOnlyCollection<EmailAddress> To { get; init; }
    public IReadOnlyCollection<EmailAddress> Cc { get; init; } = [];
    public IReadOnlyCollection<EmailAddress> Bcc { get; init; } = [];
    public IReadOnlyCollection<EmailAddress> ReplyTo { get; init; } = [];
    public IReadOnlyCollection<EmailAttachment> Attachments { get; init; } = [];
    public EmailPriority Priority { get; init; } = EmailPriority.Normal;
}
