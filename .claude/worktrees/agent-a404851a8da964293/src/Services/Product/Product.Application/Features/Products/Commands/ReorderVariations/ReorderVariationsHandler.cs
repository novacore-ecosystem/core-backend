using NovaCore.Product.Application.Abstractions.Persistence.Products;

namespace NovaCore.Product.Application.Features.Products.Commands.ReorderVariations;

public sealed class ReorderVariationsHandler(IProductWriteService productWriteService)
    : ICommandHandler<ReorderVariationsCommand, ReorderVariationsResponse>
{
    public async Task<ReorderVariationsResponse> Handle(
        ReorderVariationsCommand request,
        CancellationToken ct = default)
    {
        await productWriteService.ReorderVariationsAsync(
            request.ProductId,
            request.OrderedVariationIds,
            ct);

        return new ReorderVariationsResponse();
    }
}
