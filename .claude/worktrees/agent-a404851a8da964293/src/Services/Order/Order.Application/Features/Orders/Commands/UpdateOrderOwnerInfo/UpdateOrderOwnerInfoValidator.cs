using FluentValidation;

namespace NovaCore.Order.Application.Features.Orders.Commands.UpdateOrderOwnerInfo;

/// <summary>
/// OwnerEmail/OwnerPhone are Value Objects by the time they reach here - Email.Create/
/// PhoneNumber.Create already rejected an invalid value at the API adapter, so this only
/// validates what a single Value Object can't (OrderId's presence).
/// </summary>
public sealed class UpdateOrderOwnerInfoValidator : AbstractValidator<UpdateOrderOwnerInfoCommand>
{
    public UpdateOrderOwnerInfoValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required");
    }
}
