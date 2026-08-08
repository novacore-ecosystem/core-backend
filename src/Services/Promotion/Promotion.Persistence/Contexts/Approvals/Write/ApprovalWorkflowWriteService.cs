using NovaCore.Promotion.Application.Abstractions.Persistence.Approvals;
using NovaCore.Promotion.Persistence.Contexts.Approvals.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Approvals.Write;

/// <summary>
/// Every method here stages only - never a bare SaveChangesAsync. All three are always called
/// alongside a matching IPromotionWriteService call (Submit/Approve/Reject a Promotion spans two
/// independent aggregate roots), so the Handler - never this service - owns the single
/// ExecuteTransactionAsync that commits both. See SubmitPromotionHandler/ApprovePromotionHandler/
/// RejectPromotionHandler.
/// </summary>
public sealed class ApprovalWorkflowWriteService(IApprovalWorkflowRepository approvalWorkflowRepo)
    : IApprovalWorkflowWriteService
{
    public Task CreateAsync(ApprovalWorkflow workflow, CancellationToken ct = default) =>
        approvalWorkflowRepo.AddAsync(workflow, ct);

    public Task ApproveAsync(Guid workflowId, CancellationToken ct = default) =>
        approvalWorkflowRepo.UpdateAsync(workflowId, workflow => workflow.Approve(), ct);

    public Task RejectAsync(Guid workflowId, CancellationToken ct = default) =>
        approvalWorkflowRepo.UpdateAsync(workflowId, workflow => workflow.Reject(), ct);
}
