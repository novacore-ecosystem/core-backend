using NovaCore.Promotion.Application.Abstractions.Persistence.Approvals;
using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

namespace NovaCore.Promotion.Application.Features.Promotions.Commands.ApprovePromotion;

public sealed class ApprovePromotionHandler(
    IPromotionReadService promotionReadService,
    IPromotionWriteService promotionWriteService,
    IApprovalWorkflowWriteService approvalWorkflowWriteService,
    IUnitOfWork uow) : ICommandHandler<ApprovePromotionCommand, ApprovePromotionResponse>
{
    public async Task<ApprovePromotionResponse> Handle(ApprovePromotionCommand request, CancellationToken ct = default)
    {
        var promotion = await promotionReadService.GetByIdAsync(request.PromotionId, ct)
            ?? throw new NotFoundException(nameof(PromotionEntity), request.PromotionId);

        var workflowId = promotion.ApprovalWorkflowId
            ?? throw new BusinessRuleException(MessageCode.BadRequest, "Promotion has not been submitted for approval.");

        await uow.ExecuteTransactionAsync(async () =>
        {
            await approvalWorkflowWriteService.ApproveAsync(workflowId, ct);
            await promotionWriteService.ApproveAsync(request.PromotionId, ct);
        }, ct: ct);

        return new ApprovePromotionResponse();
    }
}
