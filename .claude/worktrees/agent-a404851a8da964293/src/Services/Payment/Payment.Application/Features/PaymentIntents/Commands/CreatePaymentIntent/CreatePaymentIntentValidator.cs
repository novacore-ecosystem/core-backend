using FluentValidation;

namespace NovaCore.Payment.Application.Features.PaymentIntents.Commands.CreatePaymentIntent;

public sealed class CreatePaymentIntentValidator : AbstractValidator<CreatePaymentIntentCommand>
{
    public CreatePaymentIntentValidator()
    {
        RuleFor(x => x.ReferenceId)
            .NotEmpty().WithMessage("ReferenceId is required");

        RuleFor(x => x.RequestedAmount)
            .Must(Money.IsValid).WithMessage("RequestedAmount must be greater than or equal to zero");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("CurrencyCode is required")
            .Must(Currency.IsValid).WithMessage("CurrencyCode must be a 3-letter ISO-4217 code");
    }
}
