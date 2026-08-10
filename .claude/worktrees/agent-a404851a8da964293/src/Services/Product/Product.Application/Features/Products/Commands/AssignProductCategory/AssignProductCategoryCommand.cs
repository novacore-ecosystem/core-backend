namespace NovaCore.Product.Application.Features.Products.Commands.AssignProductCategory;

public sealed record AssignProductCategoryCommand(Guid ProductId, Guid CategoryId) : ICommand<AssignProductCategoryResponse>;

public sealed record AssignProductCategoryResponse;
