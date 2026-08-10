namespace NovaCore.Product.Application.Features.Products.Commands.ReorderVariations;

public sealed record ReorderVariationsCommand(
    Guid ProductId,
    IReadOnlyList<Guid> OrderedVariationIds) : ICommand<ReorderVariationsResponse>;

public sealed record ReorderVariationsResponse;
