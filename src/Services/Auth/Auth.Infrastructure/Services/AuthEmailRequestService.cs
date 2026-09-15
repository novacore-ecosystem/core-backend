using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Security.PasswordReset;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Configurations;
using NovaCore.Auth.Infrastructure.Configurations.Settings;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.Infrastructure.Services;

public sealed class AuthEmailRequestService(
    IAccountReadService accountReadService,
    IAuthService authService,
    IPasswordResetTokenService passwordResetTokenService,
    ClientSetting clientSetting,
    IOutboxStore outboxStore,
    IUnitOfWork unitOfWork,
    IEmailResendCooldown cooldown,
    EmailResendCooldownSetting cooldownSetting,
    IAppLogger<AuthEmailRequestService> logger) : IAuthEmailRequestService, IAppService
{
    public async Task RequestEmailVerificationAsync(string email, CancellationToken ct = default)
    {
        var account = await accountReadService.GetByEmailAsync(email, ct);
        if (account is null || account.EmailConfirmed)
        {
            logger.Information("Email verification requested for an unconfirmable account.");
            return;
        }

        var token = await authService.GenerateEmailConfirmationTokenAsync(account.Id, ct);
        var verificationLink = $"{clientSetting.EmailVerificationUrl}?token={Uri.EscapeDataString(token)}&accountId={account.Id}";

        var @event = new EmailVerificationRequestedIntegrationEvent(account.Id.ToString(), account.Email!, verificationLink);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await outboxStore.EnqueueAsync(@event, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }, ct: ct);
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken ct = default)
    {
        var account = await accountReadService.GetByEmailAsync(email, ct);
        if (account is null)
        {
            logger.Information("Password reset requested for an email with no matching account.");
            return;
        }

        var resetToken = await passwordResetTokenService.GenerateTokenAsync(account.Id, ct);
        var resetLink = $"{clientSetting.ResetPasswordUrl}?token={Uri.EscapeDataString(resetToken)}";

        var @event = new PasswordResetRequestedIntegrationEvent(
            account.Id.ToString(),
            email,
            resetLink,
            CacheKeyConstant.PasswordResetTokens.DefaultTtlMinutes);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await outboxStore.EnqueueAsync(@event, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }, ct: ct);

        logger.Information("Password reset requested for account {AccountId}.", account.Id);
    }

    public Task<CooldownClaim> TryDispatchEmailVerificationAsync(string email, CancellationToken ct = default) =>
        TryDispatchAsync(AuthMailPurpose.EmailVerification, email, () => RequestEmailVerificationAsync(email, ct), ct);

    public Task<CooldownClaim> TryDispatchPasswordResetAsync(string email, CancellationToken ct = default) =>
        TryDispatchAsync(AuthMailPurpose.PasswordReset, email, () => RequestPasswordResetAsync(email, ct), ct);

    /// <summary>
    /// The single source of truth for the email-resend cooldown: claims the per-(purpose, email)
    /// Redis window, and only if claimed, runs <paramref name="sendAsync"/> - releasing the claim
    /// again if that throws, so a failed attempt never costs the caller the cooldown window.
    /// </summary>
    private async Task<CooldownClaim> TryDispatchAsync(
        AuthMailPurpose purpose,
        string email,
        Func<Task> sendAsync,
        CancellationToken ct)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var cooldownKey = $"{purpose}:{normalizedEmail}";

        var claim = await cooldown.TryClaimAsync(cooldownKey, cooldownSetting.Window, ct);
        if (!claim.Claimed)
            return claim;

        try
        {
            await sendAsync();
        }
        catch
        {
            await cooldown.ReleaseAsync(cooldownKey, ct);
            throw;
        }

        return claim;
    }
}
