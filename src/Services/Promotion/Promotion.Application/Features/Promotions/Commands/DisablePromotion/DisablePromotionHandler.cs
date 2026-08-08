using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

namespace NovaCore.Promotion.Application.Features.Promotions.Commands.DisablePromotion;

public sealed class DisablePromotionHandler(
    IPromotionReadService promotionReadService,
    IPromotionWriteService promotionWriteService) : ICommandHandler<DisablePromotionCommand, DisablePromotionResponse>
{
    public async Task<DisablePromotionResponse> Handle(DisablePromotionCommand request, CancellationToken ct = default)
    {
        _ = await promotionReadService.GetByIdAsync(request.PromotionId, ct)
            ?? throw new NotFoundException(nameof(PromotionEntity), request.PromotionId);

        await promotionWriteService.DisableAsync(request.PromotionId, ct);

        return new DisablePromotionResponse();
    }
}
