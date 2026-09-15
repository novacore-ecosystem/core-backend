using NovaCore.Auth.Application.Abstractions.Auth;

using NovaCore.BuildingBlock.Domain.Enums;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ResendEmail;

/// <summary>
/// Single resend entry point for every authentication email purpose - reuses
/// <see cref="IAuthEmailRequestService"/>'s claim-then-send flow rather than duplicating
/// Register/ForgotPassword's token-generation logic or the cooldown check itself, so the same
/// address cannot be resent to more than once every cooldown window (also the window Register's
/// initial send establishes).
/// </summary>
public sealed class ResendEmailHandler(
    IAuthEmailRequestService authEmailRequestService) : ICommandHandler<ResendEmailCommand>
{
    public async Task Handle(ResendEmailCommand request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var claim = request.Purpose switch
        {
            AuthMailPurpose.EmailVerification => await authEmailRequestService.TryDispatchEmailVerificationAsync(email, ct),
            AuthMailPurpose.PasswordReset => await authEmailRequestService.TryDispatchPasswordResetAsync(email, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Purpose), request.Purpose, "Unsupported resend purpose."),
        };

        if (!claim.Claimed)
            throw new BadRequestException(
                MessageCode.EmailResendCooldown,
                new { remainingSeconds = claim.RemainingSeconds });
    }
}
