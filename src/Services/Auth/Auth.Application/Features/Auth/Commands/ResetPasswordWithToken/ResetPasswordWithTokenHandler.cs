using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Security.PasswordReset;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Common;

using NovaCore.BuildingBlock.Domain.Enums;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ResetPasswordWithToken;

/// <summary>
/// Completes the "forgot password" flow. Invalid/expired tokens surface the same generic error
/// as an unknown account, so a caller who found this token by guessing learns nothing about
/// whether it ever existed. Guarded by <see cref="IInvalidAttemptGuard"/>, keyed by the submitted
/// token itself - this request carries no email/account identifier until the token resolves one,
/// so the token string is the only stable dimension available to lock on.
/// </summary>
public sealed class ResetPasswordWithTokenHandler(
    IPasswordResetTokenService passwordResetTokenService,
    IAuthService authService,
    IAccountReadService accountReadService,
    IAccountWriteService accountWriteService,
    IRefreshTokenService refreshTokenService,
    IInvalidAttemptGuard attemptGuard,
    IUnitOfWork unitOfWork) : ICommandHandler<ResetPasswordWithTokenCommand>
{
    private const string InvalidTokenMessage = "Invalid or expired reset token.";

    public async Task Handle(ResetPasswordWithTokenCommand request, CancellationToken ct = default)
    {
        var lockKey = $"password-reset:{request.Token}";

        var lockStatus = await attemptGuard.CheckLockAsync(lockKey, ct);
        if (lockStatus.Locked)
            throw new BadRequestException(MessageCode.VerificationCodeLocked, new { remainingSeconds = lockStatus.RemainingSeconds });

        // Validate the reset token
        var (accountId, isValid) = await passwordResetTokenService.ValidateTokenAsync(request.Token, ct);
        if (!isValid)
        {
            var result = await attemptGuard.RecordFailureAsync(lockKey, VerificationAttemptPolicy.MaxInvalidAttempts, VerificationAttemptPolicy.LockDuration, ct);
            if (result.Locked)
                throw new BadRequestException(MessageCode.VerificationCodeLocked, new { remainingSeconds = result.RemainingSeconds });

            throw new BadRequestException(InvalidTokenMessage);
        }

        var account = await accountReadService.GetByIdAsync(accountId, ct)
            ?? throw new BadRequestException(InvalidTokenMessage);

        // Force-set the new password (no current password to verify against) and update Identity's hash
        var updated = await authService.SetPasswordAsync(account.Id, request.NewPassword, ct);
        if (!updated)
            throw new BadRequestException(InvalidTokenMessage);

        // Record password history against the freshly-updated hash
        var refreshedAccount = await accountReadService.GetByIdAsync(account.Id, ct)
            ?? throw new BadRequestException(InvalidTokenMessage);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await accountWriteService.RecordPasswordChangeAsync(account.Id, refreshedAccount.PasswordHash!, ct);
        }, ct: ct);

        // Single-use token, invalidate it, and revoke every existing session for this account
        await passwordResetTokenService.InvalidateTokenAsync(request.Token, ct);
        await refreshTokenService.RevokeAllUserTokensAsync(account.Id, ct);
        await attemptGuard.ResetAsync(lockKey, ct);
    }
}
