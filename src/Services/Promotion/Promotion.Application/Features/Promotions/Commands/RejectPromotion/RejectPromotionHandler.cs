using NovaCore.Promotion.Application.Abstractions.Persistence.Approvals;
using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

namespace NovaCore.Promotion.Application.Features.Promotions.Commands.RejectPromotion;

public sealed class RejectPromotionHandler(
    IPromotionReadService promotionReadService,
    IPromotionWriteService promotionWriteService,
    IApprovalWorkflowWriteService approvalWorkflowWriteService,
    IUnitOfWork uow) : ICommandHandler<RejectPromotionCommand, RejectPromotionResponse>
{
    public async Task<RejectPromotionResponse> Handle(RejectPromotionCommand request, CancellationToken ct = default)
    {
        var promotion = await promotionReadService.GetByIdAsync(request.PromotionId, ct)
            ?? throw new NotFoundException(nameof(PromotionEntity), request.PromotionId);

        var workflowId = promotion.ApprovalWorkflowId
            ?? throw new BusinessRuleException(MessageCode.BadRequest, "Promotion has not been submitted for approval.");

        await uow.ExecuteTransactionAsync(async () =>
        {
            await approvalWorkflowWriteService.RejectAsync(workflowId, ct);
            await promotionWriteService.RejectAsync(request.PromotionId, ct);
        }, ct: ct);

        return new RejectPromotionResponse();
    }
}
