using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Approvals.Repositories;

public sealed class ApprovalWorkflowRepo(PromotionDbContext dbContext)
    : PromotionBaseRepository<ApprovalWorkflow, Guid>(dbContext), IApprovalWorkflowRepository
{
}
