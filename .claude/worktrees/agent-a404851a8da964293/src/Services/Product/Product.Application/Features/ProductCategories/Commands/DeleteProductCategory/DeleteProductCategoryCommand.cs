namespace NovaCore.Product.Application.Features.ProductCategories.Commands.DeleteProductCategory;

public sealed record DeleteProductCategoryCommand(Guid ProductCategoryId) : ICommand<DeleteProductCategoryResponse>;

public sealed record DeleteProductCategoryResponse;
