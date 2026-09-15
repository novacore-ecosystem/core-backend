using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Services;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordHandler(
    ICurrentUserService currentUserService,
    IAuthService authService,
    IAccountReadService accountReadService,
    IAccountWriteService accountWriteService,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork) : ICommandHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken ct = default)
    {
        var accountId = currentUserService.GetUserId()
            ?? throw new UnauthorizedException("User is not authenticated.");

        // Validate current password and update Identity's password hash
        var updated = await authService.UpdatePasswordAsync(accountId, request.CurrentPassword, request.NewPassword, ct);
        if (!updated)
            throw new UnauthorizedException("Current password is incorrect.");

        // Record password history against the freshly-updated hash
        var account = await accountReadService.GetByIdAsync(accountId, ct)
            ?? throw new NotFoundException("Account", accountId);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await accountWriteService.RecordPasswordChangeAsync(accountId, account.PasswordHash!, ct);
        }, ct: ct);

        // Invalidate every other active session for this account
        await refreshTokenService.RevokeAllUserTokensAsync(accountId, ct);

        currentUserService.RemoveAccessToken();
        currentUserService.RemoveRefreshToken();
    }
}
