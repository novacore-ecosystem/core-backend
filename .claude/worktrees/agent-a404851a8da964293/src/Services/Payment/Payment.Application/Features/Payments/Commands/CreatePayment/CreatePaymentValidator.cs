using FluentValidation;

namespace NovaCore.Payment.Application.Features.Payments.Commands.CreatePayment;

public sealed class CreatePaymentValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.ReferenceId)
            .NotEmpty().WithMessage("ReferenceId is required");

        RuleFor(x => x.Amount)
            .Must(Money.IsValid).WithMessage("Amount must be greater than or equal to zero");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("CurrencyCode is required")
            .Must(Currency.IsValid).WithMessage("CurrencyCode must be a 3-letter ISO-4217 code");

        RuleFor(x => x.GatewayId)
            .NotEmpty().WithMessage("GatewayId is required");
    }
}
