using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Logging;

using NovaCore.BuildingBlock.Infrastructure.Mail.Abstractions;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Internal;

internal sealed class MailKitEmailSender(
    MailOptions options,
    ILogger<MailKitEmailSender> logger) : IEmailSender
{
    public async Task<EmailResult> SendAsync(
        EmailMessage message,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var validationErrors = EmailMessageValidator.Validate(message, options);
        if (validationErrors.Count > 0)
        {
            var validationError = string.Join(" ", validationErrors);
            logger.LogWarning(
                "Email send validation failed for subject '{Subject}': {ValidationError}",
                message.Subject,
                validationError);

            return EmailResult.Failure(validationError);
        }

        logger.LogInformation(
            "Sending email '{Subject}' to {RecipientCount} recipient(s)",
            message.Subject,
            message.To.Count);

        try
        {
            using var mimeMessage = MailKitMessageFactory.Create(message, options);
            using var client = new SmtpClient();

            await client.ConnectAsync(
                options.Host,
                options.Port,
                options.UseSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None,
                ct);

            if (!string.IsNullOrWhiteSpace(options.Username))
                await client.AuthenticateAsync(options.Username, options.Password ?? string.Empty, ct);

            await client.SendAsync(mimeMessage, ct);
            await client.DisconnectAsync(true, ct);

            logger.LogInformation("Email '{Subject}' sent successfully", message.Subject);

            return EmailResult.Success(mimeMessage.MessageId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to send email '{Subject}'", message.Subject);

            return EmailResult.Failure($"Failed to send email: {ex.Message}");
        }
    }
}
