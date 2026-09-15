namespace NovaCore.Auth.Application.Abstractions.Auth;

/// <summary>
/// Orchestrates the token-generation, link-building, and event-publishing steps behind Auth's
/// two outbound authentication emails. Shared by Register/ForgotPassword and their resend
/// counterparts so every entry point behaves identically and none duplicates Identity's
/// token logic.
/// </summary>
public interface IAuthEmailRequestService
{
    /// <summary>
    /// Issues an email-confirmation token and publishes the verification-email request, unless
    /// the account is missing or already confirmed - both a silent no-op, so this can be called
    /// from a resend endpoint without leaking account state.
    /// </summary>
    /// <param name="email">The account's email address.</param>
    Task RequestEmailVerificationAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Issues a password-reset token and publishes the reset-email request, unless no account
    /// matches the email - a silent no-op, so this can be called without leaking account
    /// existence.
    /// </summary>
    /// <param name="email">The email address a reset was requested for.</param>
    Task RequestPasswordResetAsync(string email, CancellationToken ct = default);
}
