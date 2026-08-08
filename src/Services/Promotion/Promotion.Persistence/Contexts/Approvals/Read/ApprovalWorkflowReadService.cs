using NovaCore.Promotion.Application.Abstractions.Persistence.Approvals;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Approvals.Read;

public sealed class ApprovalWorkflowReadService(PromotionDbContext dbContext) : IApprovalWorkflowReadService
{
}
