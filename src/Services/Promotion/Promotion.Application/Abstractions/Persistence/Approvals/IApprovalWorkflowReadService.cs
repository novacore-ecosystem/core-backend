namespace NovaCore.Promotion.Application.Abstractions.Persistence.Approvals;

public interface IApprovalWorkflowReadService
{
    Task<ApprovalWorkflow?> GetByIdAsync(Guid workflowId, CancellationToken ct = default);
}
