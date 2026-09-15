using NovaCore.Auth.Application.Configurations;

using FluentValidation;

namespace NovaCore.Auth.Infrastructure.Configurations.Validators;

/// <summary>
/// Fail-fast rules for <see cref="ClientSetting"/>. Lives in Auth.Application (ForgotPasswordHandler/
/// RegisterHandler read it directly) and isn't an ISetting, so ConfigurationExtensions validates it
/// manually against the same section, the same way it already does for RootSetting.
/// </summary>
public sealed class ClientSettingValidator : AbstractValidator<ClientSetting>
{
    public ClientSettingValidator()
    {
        RuleFor(x => x.ResetPasswordUrl).NotEmpty().WithMessage("ResetPasswordUrl is required.");
        RuleFor(x => x.EmailVerificationUrl).NotEmpty().WithMessage("EmailVerificationUrl is required.");
    }
}
