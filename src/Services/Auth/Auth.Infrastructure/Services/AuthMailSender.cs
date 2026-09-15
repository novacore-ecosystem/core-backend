using NovaCore.Auth.Application.Abstractions.Auth;

using NovaCore.BuildingBlock.Infrastructure.Mail.Abstractions;
using NovaCore.BuildingBlock.Infrastructure.Mail.Builders;
using NovaCore.BuildingBlock.Infrastructure.Mail.Models;

namespace NovaCore.Auth.Infrastructure.Services;

/// <summary>
/// Renders and sends Auth's transactional emails through the shared mail building block -
/// Resend stays wrapped behind <see cref="IEmailSender"/>, Application never sees it.
/// </summary>
public sealed class AuthMailSender(IEmailSender emailSender) : IAuthMailSender, IAppService
{
    public Task SendEmailVerificationAsync(string email, string verificationLink, CancellationToken ct = default)
    {
        var htmlBody = ResolveEmailVerificationTemplate(verificationLink) ?? BuildFallbackEmailVerificationBody(verificationLink);

        var message = new EmailMessage
        {
            Subject = "Verify your email address",
            HtmlBody = htmlBody,
            To = [new EmailAddress(email)],
        };

        return emailSender.SendAsync(message, ct);
    }

    /// <summary>
    /// Operator-managed templates aren't implemented yet - always falls through to
    /// <see cref="BuildFallbackEmailVerificationBody"/>. Replace this lookup with a real
    /// template-entity read (keyed by purpose/locale/tenant) without touching the consumer or
    /// the fallback body below.
    /// </summary>
    private static string? ResolveEmailVerificationTemplate(string verificationLink) => null;

    private static string BuildFallbackEmailVerificationBody(string verificationLink) =>
        EmailTemplate.Default.Wrap(EmailBodyBuilder.Create()
            .Heading("Verify your email address")
            .Paragraph("Thanks for signing up! Click the button below to confirm your email address.")
            .Button("Verify email", verificationLink)
            .SmallText("If you didn't create a NovaCore account, you can safely ignore this email.")
            .Build());
}
