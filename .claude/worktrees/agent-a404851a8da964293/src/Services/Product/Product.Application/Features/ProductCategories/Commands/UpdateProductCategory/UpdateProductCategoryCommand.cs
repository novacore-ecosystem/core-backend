namespace NovaCore.Product.Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed record UpdateProductCategoryCommand(
    Guid ProductCategoryId,
    string Name,
    string Description,
    Guid? ParentCategoryId = null) : ICommand<UpdateProductCategoryResponse>;

public sealed record UpdateProductCategoryResponse;
