using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.Product.Application.Abstractions.Persistence.ProductCategories;

namespace NovaCore.Product.Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed class CreateProductCategoryHandler(
    ICurrentLocaleService currentLocaleService,
    IProductCategoryReadService categoryReadService,
    IProductCategoryWriteService categoryWriteService) : ICommandHandler<CreateProductCategoryCommand, CreateProductCategoryResponse>
{
    public async Task<CreateProductCategoryResponse> Handle(CreateProductCategoryCommand request, CancellationToken ct = default)
    {
        if (await categoryReadService.CodeExistsAsync(request.Code, ct))
            throw new ConflictException($"ProductCategory with code ({request.Code}) already exists");

        if (request.ParentCategoryId is not null)
        {
            _ = await categoryReadService.GetByIdAsync(request.ParentCategoryId.Value, ct)
                ?? throw new NotFoundException(nameof(ProductCategory), request.ParentCategoryId.Value);
        }

        var category = ProductCategory.Create(
            Guid.CreateVersion7(),
            CategoryCode.Create(request.Code),
            request.Name,
            request.Description,
            request.ParentCategoryId,
            note: request.Note ?? string.Empty);
        var locale = currentLocaleService.GetLocale();
        category.Translate(
            LanguageCode.Create(locale),
            request.Name,
            request.Description);
        await categoryWriteService.CreateAsync(category, ct);

        return new CreateProductCategoryResponse(category.Id);
    }
}
