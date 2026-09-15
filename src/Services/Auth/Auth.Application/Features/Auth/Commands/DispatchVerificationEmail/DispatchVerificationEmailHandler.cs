using NovaCore.Auth.Application.Abstractions.Auth;

namespace NovaCore.Auth.Application.Features.Auth.Commands.DispatchVerificationEmail;

/// <summary>
/// Consumer-driven counterpart of Register's own synchronous dispatch call - calls the exact same
/// <see cref="IAuthEmailRequestService.TryDispatchEmailVerificationAsync"/> flow. Whichever of the
/// two (Register's synchronous call or this, triggered by <c>UserRegisteredIntegrationEvent</c>)
/// reaches the cooldown claim first sends the email; the other finds the cooldown already claimed
/// and no-ops - this is expected, not an error, so a lost race is never surfaced as a failure.
/// </summary>
public sealed class DispatchVerificationEmailHandler(
    IAuthEmailRequestService authEmailRequestService,
    IAppLogger<DispatchVerificationEmailHandler> logger) : ICommandHandler<DispatchVerificationEmailCommand>
{
    public async Task Handle(DispatchVerificationEmailCommand request, CancellationToken ct = default)
    {
        var claim = await authEmailRequestService.TryDispatchEmailVerificationAsync(request.Email, ct);

        if (!claim.Claimed)
            logger.Information(
                "Skipped verification email dispatch for {Email} - already dispatched.",
                request.Email);
    }
}
