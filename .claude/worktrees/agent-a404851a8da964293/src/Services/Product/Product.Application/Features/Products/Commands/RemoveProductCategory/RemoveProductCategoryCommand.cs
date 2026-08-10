namespace NovaCore.Product.Application.Features.Products.Commands.RemoveProductCategory;

public sealed record RemoveProductCategoryCommand(Guid ProductId, Guid CategoryId) : ICommand<RemoveProductCategoryResponse>;

public sealed record RemoveProductCategoryResponse;
