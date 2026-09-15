using NovaCore.Auth.Application.Abstractions.Security.PasswordReset;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Always succeeds regardless of whether the email belongs to an account - deliberately avoids
/// account enumeration. A reset token is only generated (and, once notification dispatch exists,
/// only sent) when the account is actually found.
/// </summary>
public sealed class ForgotPasswordHandler(
    IAccountReadService accountReadService,
    IPasswordResetTokenService passwordResetTokenService,
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

        // TODO: no existing notification/email dispatch mechanism found in the codebase — wire
        // this to whichever service owns outbound email once identified. `resetToken` is the
        // value that needs to reach the account's email as (or embedded in) the reset link.
        // Deliberately not logged at any level - it is a live credential.
        logger.Information("Password reset token generated for account {AccountId}.", account.Id);
        _ = resetToken;
    }
}
