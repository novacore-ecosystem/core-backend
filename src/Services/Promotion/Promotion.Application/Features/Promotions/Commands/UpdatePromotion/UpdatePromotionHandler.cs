using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

namespace NovaCore.Promotion.Application.Features.Promotions.Commands.UpdatePromotion;

public sealed class UpdatePromotionHandler(
    IPromotionReadService promotionReadService,
    IPromotionWriteService promotionWriteService,
    IUnitOfWork uow) : ICommandHandler<UpdatePromotionCommand, UpdatePromotionResponse>
{
    public async Task<UpdatePromotionResponse> Handle(UpdatePromotionCommand request, CancellationToken ct = default)
    {
        _ = await promotionReadService.GetByIdAsync(request.PromotionId, ct)
            ?? throw new NotFoundException(nameof(PromotionEntity), request.PromotionId);

        await uow.ExecuteTransactionAsync(async () =>
        {
            await promotionWriteService.UpdateDetailsAsync(
                request.PromotionId,
                request.Name,
                request.Description,
                request.StartTime,
                request.EndTime,
                request.TimeZone,
                request.Priority,
                ct);
        }, ct: ct);

        return new UpdatePromotionResponse();
    }
}
