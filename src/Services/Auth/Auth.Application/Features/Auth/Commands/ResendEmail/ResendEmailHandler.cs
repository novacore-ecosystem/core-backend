using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Services;

using NovaCore.BuildingBlock.Domain.Enums;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ResendEmail;

/// <summary>
/// Single resend entry point for every authentication email purpose - reuses
/// <see cref="IAuthEmailRequestService"/> rather than duplicating Register/ForgotPassword's
/// token-generation logic, and enforces a strict per-email, per-purpose cooldown so the same
/// address cannot be resent to more than once every 30 seconds.
/// </summary>
public sealed class ResendEmailHandler(
    IEmailResendCooldown cooldown,
    IAuthEmailRequestService authEmailRequestService) : ICommandHandler<ResendEmailCommand>
{
    private static readonly TimeSpan CooldownWindow = TimeSpan.FromSeconds(30);

    public async Task Handle(ResendEmailCommand request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var cooldownKey = $"{request.Purpose}:{email}";

        var claim = await cooldown.TryClaimAsync(cooldownKey, CooldownWindow, ct);
        if (!claim.Claimed)
            throw new BadRequestException(
                MessageCode.EmailResendCooldown,
                new { remainingSeconds = claim.RemainingSeconds });

        try
        {
            switch (request.Purpose)
            {
                case AuthMailPurpose.EmailVerification:
                    await authEmailRequestService.RequestEmailVerificationAsync(email, ct);
                    break;

                case AuthMailPurpose.PasswordReset:
                    await authEmailRequestService.RequestPasswordResetAsync(email, ct);
                    break;
            }
        }
        catch
        {
            // The attempt never actually queued an email - don't make the caller wait out a
            // cooldown for a request that failed before being accepted for delivery.
            await cooldown.ReleaseAsync(cooldownKey, ct);
            throw;
        }
    }
}
