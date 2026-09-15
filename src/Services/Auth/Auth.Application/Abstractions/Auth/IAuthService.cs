namespace NovaCore.Auth.Application.Abstractions.Auth;

public interface IAuthService
{
    Task<Account?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);

    Task<Account?> GetUserByEmailAsync(string email, CancellationToken ct = default);

    Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);

    /// <summary>
    /// Validates a password against an already-resolved Account, without re-looking it up
    /// by email. Login must resolve the Account itself first (tenant-scoped - see
    /// IAccountReadService.GetByEmailAsync), since the plain email-only lookup this type wraps
    /// (UserManager.FindByEmailAsync) has no tenant awareness and could resolve a different
    /// account than the one the caller's TenantClient PublicKey actually authorized.
    /// </summary>
    Task<bool> ValidateCredentialsAsync(Account account, string password, CancellationToken ct = default);

    Task<Account?> CreateUserAsync(
        string email,
        string username,
        string password,
        CancellationToken ct = default);

    /// <summary>
    /// Creates an Account with an explicit id, shared with a UserProfile already created
    /// in the User service (admin/root-initiated user creation).
    /// </summary>
    Task<Account?> CreateUserAsync(
        Guid id,
        string email,
        string username,
        string password,
        CancellationToken ct = default);

    Task<bool> UpdatePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken ct = default);

    /// <summary>
    /// Force-sets a new password without knowing the current one, for the token-based "forgot
    /// password" completion flow (ResetPasswordWithTokenHandler) - the possession of a valid,
    /// unexpired reset token stands in for knowing the current password. Backed by
    /// UserManager.RemovePasswordAsync + AddPasswordAsync, the standard ASP.NET Identity pattern
    /// for an administrative/token-authorized password reset, so hashing still goes through
    /// Identity rather than being hand-rolled here.
    /// </summary>
    Task<bool> SetPasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken ct = default);

    Task<bool> ConfirmEmailAsync(Guid userId, CancellationToken ct = default);

    Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct = default);
}
