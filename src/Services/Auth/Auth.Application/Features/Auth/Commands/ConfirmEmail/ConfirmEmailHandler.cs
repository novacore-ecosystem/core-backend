using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Common;

using NovaCore.BuildingBlock.Domain.Enums;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ConfirmEmail;

/// <summary>
/// Completes the registration email-verification flow using Identity's own confirmation token -
/// no extra token store, see IAuthService.GenerateEmailConfirmationTokenAsync/ConfirmEmailAsync.
/// Guarded by <see cref="IInvalidAttemptGuard"/>, keyed by AccountId (the one stable identifier
/// this request actually carries), so repeated wrong guesses against the same account lock out
/// after <see cref="VerificationAttemptPolicy.MaxInvalidAttempts"/> tries.
/// </summary>
public sealed class ConfirmEmailHandler(
    IAuthService authService,
    IInvalidAttemptGuard attemptGuard) : ICommandHandler<ConfirmEmailCommand>
{
    public async Task Handle(ConfirmEmailCommand request, CancellationToken ct = default)
    {
        var lockKey = $"email-verification:{request.AccountId}";

        var lockStatus = await attemptGuard.CheckLockAsync(lockKey, ct);
        if (lockStatus.Locked)
            throw new BadRequestException(MessageCode.VerificationCodeLocked, new { remainingSeconds = lockStatus.RemainingSeconds });

        var confirmed = await authService.ConfirmEmailAsync(request.AccountId, request.Token, ct);
        if (!confirmed)
        {
            var result = await attemptGuard.RecordFailureAsync(lockKey, VerificationAttemptPolicy.MaxInvalidAttempts, VerificationAttemptPolicy.LockDuration, ct);
            if (result.Locked)
                throw new BadRequestException(MessageCode.VerificationCodeLocked, new { remainingSeconds = result.RemainingSeconds });

            throw new BadRequestException("Invalid or expired verification token.");
        }

        await attemptGuard.ResetAsync(lockKey, ct);
    }
}
