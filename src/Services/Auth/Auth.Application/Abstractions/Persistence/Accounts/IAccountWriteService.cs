namespace NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

public interface IAccountWriteService
{
    Task DeleteIfExistAsync(Guid id, CancellationToken ct = default);

    Task SetLevelAsync(Guid id, int level, CancellationToken ct = default);

    /// <summary>
    /// Non-committing: stages the PasswordHistory row and the Account's denormalized PasswordHash
    /// copy on the change tracker only. Callers (ResetPassword/ResetPasswordWithToken) own the
    /// surrounding IUnitOfWork.ExecuteTransactionAsync themselves, since the Identity password
    /// hash update they perform first is a separate write this call must commit alongside.
    /// </summary>
    Task RecordPasswordChangeAsync(Guid accountId, string passwordHash, CancellationToken ct = default);
}
