using MimeKit;
using MimeKit.Utils;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Internal;

internal static class MailKitMessageFactory
{
    public static MimeMessage Create(EmailMessage message, MailOptions options)
    {
        var from = message.From ?? new EmailAddress(options.SenderEmail, options.SenderName);

        var mimeMessage = new MimeMessage
        {
            Subject = message.Subject,
            Priority = message.Priority.ToMessagePriority(),
        };

        mimeMessage.From.Add(from.ToMailboxAddress());
        mimeMessage.To.AddRange(message.To.ToMailboxAddresses());
        mimeMessage.Cc.AddRange(message.Cc.ToMailboxAddresses());
        mimeMessage.Bcc.AddRange(message.Bcc.ToMailboxAddresses());
        mimeMessage.ReplyTo.AddRange(message.ReplyTo.ToMailboxAddresses());
        mimeMessage.Body = BuildBody(message);

        return mimeMessage;
    }

    private static MimeEntity BuildBody(EmailMessage message)
    {
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody,
        };

        foreach (var attachment in message.Attachments)
        {
            var contentType = ContentType.Parse(attachment.ContentType);

            if (attachment.IsInline)
            {
                var resource = bodyBuilder.LinkedResources.Add(attachment.FileName, attachment.Content, contentType);
                resource.ContentId = attachment.ContentId ?? MimeUtils.GenerateMessageId();
            }
            else
            {
                bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content, contentType);
            }
        }

        return bodyBuilder.ToMessageBody();
    }
}
