namespace NovaCore.Promotion.Application.Abstractions.Persistence.Approvals;

public interface IApprovalWorkflowWriteService
{
    /// <summary>Persists an already-Submitted (Pending) workflow - see SubmitPromotionHandler. Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ApprovalWorkflow workflow, CancellationToken ct = default);

    Task ApproveAsync(Guid workflowId, CancellationToken ct = default);

    Task RejectAsync(Guid workflowId, CancellationToken ct = default);
}
