using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Security.PasswordReset;
using NovaCore.Auth.Application.Abstractions.Services;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ResetPasswordWithToken;

/// <summary>
/// Completes the "forgot password" flow. Invalid/expired tokens surface the same generic error
/// as an unknown account, so a caller who found this token by guessing learns nothing about
/// whether it ever existed.
/// </summary>
public sealed class ResetPasswordWithTokenHandler(
    IPasswordResetTokenService passwordResetTokenService,
    IAuthService authService,
    IAccountReadService accountReadService,
    IAccountWriteService accountWriteService,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork) : ICommandHandler<ResetPasswordWithTokenCommand>
{
    private const string InvalidTokenMessage = "Invalid or expired reset token.";

    public async Task Handle(ResetPasswordWithTokenCommand request, CancellationToken ct = default)
    {
        // Validate the reset token
        var (accountId, isValid) = await passwordResetTokenService.ValidateTokenAsync(request.Token, ct);
        if (!isValid)
            throw new BadRequestException(InvalidTokenMessage);

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
    }
}
