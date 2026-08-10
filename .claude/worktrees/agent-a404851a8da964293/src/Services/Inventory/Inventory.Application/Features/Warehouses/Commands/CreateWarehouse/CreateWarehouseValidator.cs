using FluentValidation;

namespace NovaCore.Inventory.Application.Features.Warehouses.Commands.CreateWarehouse;

public sealed class CreateWarehouseValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .Length(1, 50).WithMessage("Code must be between 1 and 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(1, 200).WithMessage("Name must be between 1 and 200 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .Length(1, 100).WithMessage("Country must be between 1 and 100 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .Length(1, 100).WithMessage("City must be between 1 and 100 characters");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street address is required")
            .Length(1, 200).WithMessage("Street address must be between 1 and 200 characters");

        RuleFor(x => x.StateOrProvince)
            .MaximumLength(100).WithMessage("State or province must not exceed 100 characters");

        RuleFor(x => x.District)
            .MaximumLength(100).WithMessage("District must not exceed 100 characters");

        RuleFor(x => x.Ward)
            .MaximumLength(100).WithMessage("Ward must not exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");
    }
}
