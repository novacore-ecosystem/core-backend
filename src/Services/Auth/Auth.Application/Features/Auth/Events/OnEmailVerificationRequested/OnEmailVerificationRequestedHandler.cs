using NovaCore.Auth.Application.Abstractions.Auth;

namespace NovaCore.Auth.Application.Features.Auth.Events.OnEmailVerificationRequested;

/// <summary>
/// Sends the verification email out of the request path - Register/ResendEmail only enqueue the
/// integration event that eventually reaches this handler via EmailVerificationRequestedConsumer.
/// </summary>
public sealed class OnEmailVerificationRequestedHandler(
    IAuthMailSender authMailSender,
    IAppLogger<OnEmailVerificationRequestedHandler> logger) : IInternalEventHandler<OnEmailVerificationRequestedEvent>
{
    public async Task Handle(OnEmailVerificationRequestedEvent @event, CancellationToken ct = default)
    {
        await authMailSender.SendEmailVerificationAsync(@event.Email, @event.VerificationLink, ct);

        logger.Information("Sent email verification for account {AccountId}", @event.AccountId);
    }
}
