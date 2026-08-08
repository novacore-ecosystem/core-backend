using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

namespace NovaCore.Promotion.Application.Features.Promotions.Commands.CancelPromotion;

public sealed class CancelPromotionHandler(
    IPromotionReadService promotionReadService,
    IPromotionWriteService promotionWriteService) : ICommandHandler<CancelPromotionCommand, CancelPromotionResponse>
{
    public async Task<CancelPromotionResponse> Handle(CancelPromotionCommand request, CancellationToken ct = default)
    {
        _ = await promotionReadService.GetByIdAsync(request.PromotionId, ct)
            ?? throw new NotFoundException(nameof(PromotionEntity), request.PromotionId);

        await promotionWriteService.CancelAsync(request.PromotionId, ct);

        return new CancelPromotionResponse();
    }
}
