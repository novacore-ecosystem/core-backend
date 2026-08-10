using NovaCore.BuildingBlock.Criteria.Validation;

using FluentValidation;

using NovaCore.Inventory.Application.Features.Inventories.Search;

namespace NovaCore.Inventory.Application.Features.Inventories.Queries.SearchInventoryTransactions;

public sealed class SearchInventoryTransactionsValidator : AbstractValidator<SearchInventoryTransactionsQuery>
{
    public SearchInventoryTransactionsValidator()
    {
        RuleFor(x => x.Criteria).Custom((criteria, context) =>
        {
            var errors = CriteriaRequestValidator<InventoryTransaction>.Validate(InventoryTransactionCriteriaDefinition.Instance, criteria);
            foreach (var error in errors)
                context.AddFailure(error);
        });
    }
}
