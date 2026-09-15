using NovaCore.Auth.Application.Abstractions.Auth;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Always succeeds regardless of whether the email belongs to an account - deliberately avoids
/// account enumeration. <see cref="IAuthEmailRequestService.RequestPasswordResetAsync"/> is a
/// silent no-op for an unmatched email, so this handler never has to branch on that itself.
/// </summary>
public sealed class ForgotPasswordHandler(
    IAuthEmailRequestService authEmailRequestService) : ICommandHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct = default) =>
        await authEmailRequestService.RequestPasswordResetAsync(request.Email, ct);
}
