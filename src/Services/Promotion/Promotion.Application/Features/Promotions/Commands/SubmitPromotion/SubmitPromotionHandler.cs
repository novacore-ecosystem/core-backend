using NovaCore.Promotion.Application.Abstractions.Persistence.Approvals;
using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

namespace NovaCore.Promotion.Application.Features.Promotions.Commands.SubmitPromotion;

/// <summary>
/// Creates a new ApprovalWorkflow (the general-purpose aggregate Campaign/Coupon's own approval
/// placeholders already deferred to - see Promotion.cs's doc comment) and immediately Submits it -
/// this phase exposes one "Submit Promotion" action, not a separate draft-then-submit step for the
/// workflow itself. Spans two aggregate roots in one transaction; Promotion and ApprovalWorkflow
/// still only ever reference each other by id, never by direct method call across the boundary.
/// </summary>
public sealed class SubmitPromotionHandler(
    IPromotionReadService promotionReadService,
    IPromotionWriteService promotionWriteService,
    IApprovalWorkflowWriteService approvalWorkflowWriteService,
    IUnitOfWork uow) : ICommandHandler<SubmitPromotionCommand, SubmitPromotionResponse>
{
    public async Task<SubmitPromotionResponse> Handle(SubmitPromotionCommand request, CancellationToken ct = default)
    {
        _ = await promotionReadService.GetByIdAsync(request.PromotionId, ct)
            ?? throw new NotFoundException(nameof(PromotionEntity), request.PromotionId);

        var workflow = ApprovalWorkflow.Create("Promotion");
        workflow.Submit();

        await uow.ExecuteTransactionAsync(async () =>
        {
            await approvalWorkflowWriteService.CreateAsync(workflow, ct);
            await promotionWriteService.SubmitForApprovalAsync(request.PromotionId, workflow.Id, ct);
        }, ct: ct);

        return new SubmitPromotionResponse(workflow.Id);
    }
}
