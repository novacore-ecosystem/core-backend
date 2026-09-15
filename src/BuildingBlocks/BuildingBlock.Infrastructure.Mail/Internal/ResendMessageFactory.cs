using EmailAddressList = Resend.EmailAddressList;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Internal;

internal static class ResendMessageFactory
{
    public static Resend.EmailMessage Create(EmailMessage message, ResendMailOptions options)
    {
        var from = message.From ?? new EmailAddress(options.SenderEmail, options.SenderName);

        var resendMessage = new Resend.EmailMessage
        {
            From = ToResendAddress(from),
            Subject = message.Subject,
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody,
            To = ToResendAddressList(message.To),
            Cc = ToResendAddressList(message.Cc),
            Bcc = ToResendAddressList(message.Bcc),
            ReplyTo = ToResendAddressList(message.ReplyTo),
        };

        if (message.Attachments.Count > 0)
        {
            resendMessage.Attachments = message.Attachments.Select(attachment => new Resend.EmailAttachment
            {
                Filename = attachment.FileName,
                ContentType = attachment.ContentType,
                Content = attachment.Content,
                ContentId = attachment.IsInline ? attachment.ContentId : null,
            }).ToList();
        }

        return resendMessage;
    }

    private static Resend.EmailAddress ToResendAddress(EmailAddress address) =>
        new() { Email = address.Address, DisplayName = address.Name };

    private static EmailAddressList ToResendAddressList(IReadOnlyCollection<EmailAddress> addresses) =>
        EmailAddressList.From(addresses.Select(address => address.Address));
}
