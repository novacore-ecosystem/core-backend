using MimeKit;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Internal;

internal static class EmailMessageValidator
{
    public static IReadOnlyCollection<string> Validate(EmailMessage message, MailOptions options)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(message.Subject))
            errors.Add("Subject is required.");

        if (string.IsNullOrWhiteSpace(message.HtmlBody) && string.IsNullOrWhiteSpace(message.TextBody))
            errors.Add("Either HtmlBody or TextBody must be provided.");

        var from = message.From ?? new EmailAddress(options.SenderEmail, options.SenderName);
        if (!IsValidAddress(from))
            errors.Add("A valid sender address is required.");

        if (message.To.Count == 0)
            errors.Add("At least one recipient (To) is required.");

        var invalidAddresses = message.To
            .Concat(message.Cc)
            .Concat(message.Bcc)
            .Concat(message.ReplyTo)
            .Where(address => !IsValidAddress(address))
            .Select(address => address.Address)
            .Distinct()
            .ToArray();

        if (invalidAddresses.Length > 0)
            errors.Add($"Invalid email address(es): {string.Join(", ", invalidAddresses)}");

        foreach (var attachment in message.Attachments.Where(attachment => attachment.Content.Length == 0))
            errors.Add($"Attachment '{attachment.FileName}' has no content.");

        return errors;
    }

    private static bool IsValidAddress(EmailAddress address) =>
        !string.IsNullOrWhiteSpace(address.Address) && MailboxAddress.TryParse(address.Address, out _);
}
