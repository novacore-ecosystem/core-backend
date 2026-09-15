namespace NovaCore.Auth.Application.Abstractions.Auth;

/// <summary>
/// Sends Auth's own transactional emails - the port Application depends on, implemented in
/// Infrastructure against BuildingBlock.Infrastructure.Mail so Application never sees Resend or
/// any other provider type directly.
/// </summary>
public interface IAuthMailSender
{
    /// <summary>Sends the email-verification message for a newly-registered or resent request.</summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="verificationLink">The frontend link the recipient must click to confirm the address.</param>
    Task SendEmailVerificationAsync(string email, string verificationLink, CancellationToken ct = default);
}
