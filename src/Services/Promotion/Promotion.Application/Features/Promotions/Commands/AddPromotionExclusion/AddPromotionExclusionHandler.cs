using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

namespace NovaCore.Promotion.Application.Features.Promotions.Commands.AddPromotionExclusion;

public sealed class AddPromotionExclusionHandler(
    IPromotionReadService promotionReadService,
    IPromotionWriteService promotionWriteService) : ICommandHandler<AddPromotionExclusionCommand, AddPromotionExclusionResponse>
{
    public async Task<AddPromotionExclusionResponse> Handle(AddPromotionExclusionCommand request, CancellationToken ct = default)
    {
        _ = await promotionReadService.GetByIdAsync(request.PromotionId, ct)
            ?? throw new NotFoundException(nameof(PromotionEntity), request.PromotionId);

        _ = await promotionReadService.GetByIdAsync(request.ExcludedPromotionId, ct)
            ?? throw new NotFoundException(nameof(PromotionEntity), request.ExcludedPromotionId);

        await promotionWriteService.AddExclusionAsync(request.PromotionId, request.ExcludedPromotionId, ct);

        return new AddPromotionExclusionResponse();
    }
}
