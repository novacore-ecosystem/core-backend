using NovaCore.Promotion.Application.Abstractions.Persistence.Approvals;
using NovaCore.Promotion.Persistence.Contexts.Approvals.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Approvals.Write;

public sealed class ApprovalWorkflowWriteService(IApprovalWorkflowRepository approvalWorkflowRepo) : IApprovalWorkflowWriteService
{
}
