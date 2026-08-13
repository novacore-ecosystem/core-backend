using FluentValidation;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpsertTenantTranslation;

public sealed class UpsertTenantTranslationValidator : AbstractValidator<UpsertTenantTranslationCommand>
{
    public UpsertTenantTranslationValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required.");
        RuleFor(x => x.Language).NotEmpty().WithMessage("Language is required.");
        RuleFor(x => x.Key).NotEmpty().WithMessage("Translation key is required.");
        RuleFor(x => x.Value).NotNull().WithMessage("Translation value is required.");
    }
}
