using Microsoft.Extensions.Logging;

using NovaCore.BuildingBlock.Infrastructure.Mail.Abstractions;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Internal;

internal sealed class ResendEmailSender(
    Resend.IResend resend,
    ResendMailOptions options,
    ILogger<ResendEmailSender> logger) : IEmailSender
{
    public async Task<EmailResult> SendAsync(
        EmailMessage message,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var validationErrors = EmailMessageValidator.Validate(message, options.SenderEmail, options.SenderName);
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
            "Sending email '{Subject}' to {RecipientCount} recipient(s) via Resend",
            message.Subject,
            message.To.Count);

        try
        {
            var resendMessage = ResendMessageFactory.Create(message, options);
            var response = await resend.EmailSendAsync(resendMessage, ct);

            if (!response.Success)
            {
                logger.LogError(
                    response.Exception,
                    "Failed to send email '{Subject}' via Resend",
                    message.Subject);

                return EmailResult.Failure($"Failed to send email: {response.Exception?.Message}");
            }

            logger.LogInformation("Email '{Subject}' sent successfully via Resend", message.Subject);

            return EmailResult.Success(response.Content.ToString());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to send email '{Subject}' via Resend", message.Subject);

            return EmailResult.Failure($"Failed to send email: {ex.Message}");
        }
    }
}
