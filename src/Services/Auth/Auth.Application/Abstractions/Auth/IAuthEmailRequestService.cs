using NovaCore.Auth.Application.Abstractions.Services;

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

    /// <summary>
    /// The single source of truth for the email-verification resend cooldown: atomically claims
    /// the shared per-email cooldown window and, only if claimed, calls
    /// <see cref="RequestEmailVerificationAsync"/> - releasing the claim again if that call
    /// throws. Used identically by Register's initial send and ResendEmail's explicit resend, so
    /// the cooldown this establishes is the same one either path is blocked by.
    /// </summary>
    Task<CooldownClaim> TryDispatchEmailVerificationAsync(string email, CancellationToken ct = default);

    /// <summary>Same claim-then-send-then-release-on-failure contract as <see cref="TryDispatchEmailVerificationAsync"/>, for password reset.</summary>
    Task<CooldownClaim> TryDispatchPasswordResetAsync(string email, CancellationToken ct = default);
}
