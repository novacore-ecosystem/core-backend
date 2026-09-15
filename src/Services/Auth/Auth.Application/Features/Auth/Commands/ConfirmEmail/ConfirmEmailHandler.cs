using NovaCore.Auth.Application.Abstractions.Auth;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ConfirmEmail;

/// <summary>Completes the registration email-verification flow using Identity's own confirmation token - no extra token store, see IAuthService.GenerateEmailConfirmationTokenAsync/ConfirmEmailAsync.</summary>
public sealed class ConfirmEmailHandler(IAuthService authService) : ICommandHandler<ConfirmEmailCommand>
{
    public async Task Handle(ConfirmEmailCommand request, CancellationToken ct = default)
    {
        var confirmed = await authService.ConfirmEmailAsync(request.AccountId, request.Token, ct);
        if (!confirmed)
            throw new BadRequestException("Invalid or expired verification token.");
    }
}
