using NovaCore.Promotion.Application.Abstractions.Persistence.Approvals;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Approvals.Read;

public sealed class ApprovalWorkflowReadService(PromotionDbContext dbContext) : IApprovalWorkflowReadService
{
    public async Task<ApprovalWorkflow?> GetByIdAsync(Guid workflowId, CancellationToken ct = default)
    {
        return await dbContext.ApprovalWorkflows
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == workflowId, ct);
    }
}
