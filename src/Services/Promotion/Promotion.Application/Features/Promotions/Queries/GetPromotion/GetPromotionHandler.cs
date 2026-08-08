using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

namespace NovaCore.Promotion.Application.Features.Promotions.Queries.GetPromotion;

public sealed class GetPromotionHandler(IPromotionReadService promotionReadService)
    : IQueryHandler<GetPromotionQuery, GetPromotionResponse>
{
    public async Task<GetPromotionResponse> Handle(GetPromotionQuery request, CancellationToken ct = default)
    {
        var promotion = await promotionReadService.GetByIdAsync(request.PromotionId, ct)
            ?? throw new NotFoundException(nameof(PromotionEntity), request.PromotionId);

        return new GetPromotionResponse(
            promotion.Id,
            promotion.Code.Value,
            promotion.Name,
            promotion.Description,
            promotion.Status,
            promotion.Type,
            promotion.Priority,
            promotion.Version,
            promotion.StartTime,
            promotion.EndTime,
            promotion.Currency.Value,
            promotion.TimeZone,
            promotion.IsEnabled,
            promotion.IsApproved,
            promotion.ApprovalWorkflowId,
            promotion.CampaignId,
            [.. promotion.Benefits.Select(b => new PromotionBenefitResponse(b.Id, b.BenefitType, b.Value))],
            [.. promotion.Targets.Select(t => new PromotionTargetResponse(t.Id, t.TargetType, t.TargetKey))],
            [.. promotion.Constraints.Select(c => new PromotionConstraintResponse(c.Id, c.ConstraintType, c.Value))],
            promotion.StackingPolicy?.Mode);
    }
}
