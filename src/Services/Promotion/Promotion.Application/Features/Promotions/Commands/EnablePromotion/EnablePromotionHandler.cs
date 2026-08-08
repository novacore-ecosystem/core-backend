using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

namespace NovaCore.Promotion.Application.Features.Promotions.Commands.EnablePromotion;

public sealed class EnablePromotionHandler(
    IPromotionReadService promotionReadService,
    IPromotionWriteService promotionWriteService) : ICommandHandler<EnablePromotionCommand, EnablePromotionResponse>
{
    public async Task<EnablePromotionResponse> Handle(EnablePromotionCommand request, CancellationToken ct = default)
    {
        _ = await promotionReadService.GetByIdAsync(request.PromotionId, ct)
            ?? throw new NotFoundException(nameof(PromotionEntity), request.PromotionId);

        await promotionWriteService.EnableAsync(request.PromotionId, ct);

        return new EnablePromotionResponse();
    }
}
