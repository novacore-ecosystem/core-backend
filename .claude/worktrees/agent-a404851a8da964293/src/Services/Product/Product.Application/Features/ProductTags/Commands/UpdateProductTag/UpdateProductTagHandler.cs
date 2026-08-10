using NovaCore.Product.Application.Abstractions.Persistence.ProductTags;

namespace NovaCore.Product.Application.Features.ProductTags.Commands.UpdateProductTag;

public sealed class UpdateProductTagHandler(
    IUnitOfWork uow,
    IProductTagWriteService tagWriteService) : ICommandHandler<UpdateProductTagCommand, UpdateProductTagResponse>
{
    public async Task<UpdateProductTagResponse> Handle(UpdateProductTagCommand request, CancellationToken ct = default)
    {
        await uow.ExecuteTransactionAsync(async () =>
        {
            await tagWriteService.UpdateTagNameAsync(
                request.ProductTagId,
                request.Name.Trim(),
                ct);
        }, ct: ct);

        return new UpdateProductTagResponse();
    }
}
