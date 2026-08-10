using NovaCore.Product.Application.Abstractions.Persistence.Products;

namespace NovaCore.Product.Application.Features.Products.Commands.SetDefaultVariation;

public sealed class SetDefaultVariationHandler(
    IProductWriteService productWriteService) : ICommandHandler<SetDefaultVariationCommand, SetDefaultVariationResponse>
{
    public async Task<SetDefaultVariationResponse> Handle(SetDefaultVariationCommand request, CancellationToken ct = default)
    {
        await productWriteService.SetDefaultVariationAsync(
            request.ProductId,
            request.VariationId,
            ct);

        return new SetDefaultVariationResponse();
    }
}
