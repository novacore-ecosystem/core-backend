using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Product.Application.Abstractions.Persistence.ProductTags;
using NovaCore.Product.Domain.ValueObjects;

namespace NovaCore.Product.Application.Features.ProductTags.Commands.CreateProductTag;

public sealed class CreateProductTagHandler(
    IProductTagReadService tagReadService,
    IProductTagWriteService tagWriteService) : ICommandHandler<CreateProductTagCommand, CreateProductTagResponse>
{
    public async Task<CreateProductTagResponse> Handle(CreateProductTagCommand request, CancellationToken ct = default)
    {
        if (await tagReadService.CodeExistsAsync(request.Code, ct))
            throw new ConflictException($"ProductTag with code ({request.Code}) already exists");

        var tag = ProductTag.Create(Guid.CreateVersion7(), TagCode.Create(request.Code), request.Name.Trim());
        await tagWriteService.CreateAsync(tag, ct);

        return new CreateProductTagResponse(tag.Id);
    }
}
