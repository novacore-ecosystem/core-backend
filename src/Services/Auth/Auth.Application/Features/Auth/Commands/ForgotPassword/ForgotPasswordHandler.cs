using NovaCore.Auth.Application.Abstractions.Security.PasswordReset;
using NovaCore.Auth.Application.Configurations;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Contract.Events.User;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Always succeeds regardless of whether the email belongs to an account - deliberately avoids
/// account enumeration. A reset token is only generated and the reset email only requested when
/// the account is actually found. The email itself is sent asynchronously by Notification
/// (PasswordResetRequestedIntegrationEvent via the Outbox), so this request never waits on Resend.
/// </summary>
public sealed class ForgotPasswordHandler(
    IAccountReadService accountReadService,
    IPasswordResetTokenService passwordResetTokenService,
    ClientSetting clientSetting,
    IOutboxStore outboxStore,
    IUnitOfWork unitOfWork,
    IAppLogger<ForgotPasswordHandler> logger) : ICommandHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct = default)
    {
        var account = await accountReadService.GetByEmailAsync(request.Email, ct);
        if (account is null)
        {
            logger.Information("Password reset requested for an email with no matching account.");
            return;
        }

        var resetToken = await passwordResetTokenService.GenerateTokenAsync(account.Id, ct);
        var resetLink = $"{clientSetting.ResetPasswordUrl}?token={Uri.EscapeDataString(resetToken)}";

        var @event = new PasswordResetRequestedIntegrationEvent(
            account.Id.ToString(),
            request.Email,
            resetLink,
            CacheKeyConstant.PasswordResetTokens.DefaultTtlMinutes);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await outboxStore.EnqueueAsync(@event, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }, ct: ct);

        logger.Information("Password reset requested for account {AccountId}.", account.Id);
    }
}
