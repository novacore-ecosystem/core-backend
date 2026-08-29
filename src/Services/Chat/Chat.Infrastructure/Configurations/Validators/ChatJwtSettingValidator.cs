using FluentValidation;

using NovaCore.Chat.Infrastructure.Configurations.Settings;

namespace NovaCore.Chat.Infrastructure.Configurations.Validators;

/// <summary>
/// GuestTokenGenerator only ever reads SecretKey/Issuer/Audience (it hardcodes a 2-hour expiry and
/// never touches the expiration fields), so this skips the BuildingBlock's full baseline and
/// validates only what Chat actually uses - the flexible, localized alternative to Auth's full-strict opt-in.
/// </summary>
public sealed class ChatJwtSettingValidator : AbstractValidator<ChatJwtSetting>
{
    public ChatJwtSettingValidator()
    {
        RuleFor(x => x.SecretKey).MinimumLength(32).WithMessage("SecretKey must be at least 32 characters for HMACSHA256 signing.");
        RuleFor(x => x.Issuer).NotEmpty().WithMessage("Issuer is required.");
        RuleFor(x => x.Audience).NotEmpty().WithMessage("Audience is required.");
    }
}
