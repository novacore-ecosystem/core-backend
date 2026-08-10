using FluentValidation;

namespace NovaCore.Payment.Application.Features.Refunds.Commands.CreateRefund;

public sealed class CreateRefundValidator : AbstractValidator<CreateRefundCommand>
{
    public CreateRefundValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("PaymentId is required");

        RuleFor(x => x.Amount)
            .Must(Money.IsValid).WithMessage("Amount must be greater than or equal to zero");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("CurrencyCode is required")
            .Must(Currency.IsValid).WithMessage("CurrencyCode must be a 3-letter ISO-4217 code");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500);
    }
}
