using NovaCore.Promotion.Application.Abstractions.Persistence.Approvals;
using NovaCore.Promotion.Persistence.Contexts.Approvals.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Approvals.Read;

public sealed class ApprovalWorkflowReadService(IApprovalWorkflowRepository approvalWorkflowRepo) : IApprovalWorkflowReadService
{
}
