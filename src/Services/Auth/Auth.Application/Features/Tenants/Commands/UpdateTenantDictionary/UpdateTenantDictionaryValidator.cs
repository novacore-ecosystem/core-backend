using FluentValidation;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantDictionary;

public sealed class UpdateTenantDictionaryValidator : AbstractValidator<UpdateTenantDictionaryCommand>
{
    public UpdateTenantDictionaryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required.");
        RuleFor(x => x.Language).NotEmpty().WithMessage("Language is required.");
    }
}
