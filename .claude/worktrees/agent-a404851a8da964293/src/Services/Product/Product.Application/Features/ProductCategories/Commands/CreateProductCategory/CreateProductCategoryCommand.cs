namespace NovaCore.Product.Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed record CreateProductCategoryCommand(
    string Code,
    string Name,
    string Description,
    string Note = "",
    Guid? ParentCategoryId = null) : ICommand<CreateProductCategoryResponse>;

public sealed record CreateProductCategoryResponse(Guid ProductCategoryId);
