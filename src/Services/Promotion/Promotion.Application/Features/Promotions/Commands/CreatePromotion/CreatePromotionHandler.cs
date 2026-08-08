using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

namespace NovaCore.Promotion.Application.Features.Promotions.Commands.CreatePromotion;

public sealed class CreatePromotionHandler(IPromotionWriteService promotionWriteService)
    : ICommandHandler<CreatePromotionCommand, CreatePromotionResponse>
{
    public async Task<CreatePromotionResponse> Handle(CreatePromotionCommand request, CancellationToken ct = default)
    {
        var promotion = PromotionEntity.Create(
            EntityCode.Create(request.Code),
            request.Name,
            request.Type,
            request.StartTime,
            request.EndTime,
            Currency.Create(request.Currency),
            request.TimeZone,
            request.CampaignId,
            request.Description,
            request.Priority);

        foreach (var benefit in request.Benefits ?? [])
            promotion.AddBenefit(benefit.BenefitType, benefit.Value);

        foreach (var target in request.Targets ?? [])
            promotion.AddTarget(target.TargetType, target.TargetKey);

        foreach (var constraint in request.Constraints ?? [])
            promotion.AddConstraint(constraint.ConstraintType, constraint.Value);

        promotion.SetStackingPolicy(request.StackingMode);

        await promotionWriteService.CreateAsync(promotion, ct);

        return new CreatePromotionResponse(promotion.Id);
    }
}
