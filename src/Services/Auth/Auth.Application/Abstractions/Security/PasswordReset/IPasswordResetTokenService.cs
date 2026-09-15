namespace NovaCore.Auth.Application.Abstractions.Security.PasswordReset;

public interface IPasswordResetTokenService
{
    /// <summary>
    /// Generates and caches a single-use, short-lived reset token for the given account.
    /// </summary>
    /// <param name="accountId">The account the token grants a password reset for.</param>
    /// <returns>The opaque token string to send to the account's email.</returns>
    Task<string> GenerateTokenAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Looks up the account a reset token was issued for, without consuming it.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>The owning AccountId and whether the token is currently valid (present, not expired).</returns>
    Task<(Guid AccountId, bool IsValid)> ValidateTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Invalidates a reset token so it cannot be used again (single-use enforcement).
    /// </summary>
    /// <param name="token">The token to invalidate.</param>
    Task InvalidateTokenAsync(string token, CancellationToken ct = default);
}
