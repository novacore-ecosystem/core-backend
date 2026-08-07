namespace NovaCore.Promotion.Domain.Entities.Approvals;

/// <summary>
/// Aggregate root for an ApprovalWorkflow - owns Steps. ApprovalAssignment/ApprovalDecision/
/// ApprovalComment/ApprovalHistory are related by StepId/WorkflowId only, not navigated from
/// here - see docs/promotion-service/aggregates/approval.md. This is the general-purpose
/// approval workflow structure the CampaignApproval/CouponApproval placeholders (Phase 2.1/2.2)
/// were deferring to - wiring them together is not part of this pass ("Do NOT modify previous
/// aggregates").
/// </summary>
public sealed class ApprovalWorkflow : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string WorkflowType { get; private set; } = string.Empty;
    public ApprovalWorkflowStatus Status { get; private set; }

    public ICollection<ApprovalStep> Steps { get; private set; } = [];
    public ICollection<ApprovalHistory> History { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private ApprovalWorkflow() { }

    public static ApprovalWorkflow Create(string workflowType)
    {
        if (string.IsNullOrWhiteSpace(workflowType))
            throw ExceptionFactory.RequiredField("Workflow type cannot be empty.");

        return new ApprovalWorkflow
        {
            Id = Guid.CreateVersion7(),
            WorkflowType = workflowType,
            Status = ApprovalWorkflowStatus.Draft,
        };
    }
    #endregion

    #region Status
    public void Submit()
    {
        if (Status != ApprovalWorkflowStatus.Draft)
            throw ExceptionFactory.InvalidStatus($"Cannot submit a workflow in {Status} status.");

        Status = ApprovalWorkflowStatus.Pending;
    }

    public void Approve()
    {
        if (Status != ApprovalWorkflowStatus.Pending)
            throw ExceptionFactory.InvalidStatus($"Cannot approve a workflow in {Status} status.");

        Status = ApprovalWorkflowStatus.Approved;
    }

    public void Reject()
    {
        if (Status != ApprovalWorkflowStatus.Pending)
            throw ExceptionFactory.InvalidStatus($"Cannot reject a workflow in {Status} status.");

        Status = ApprovalWorkflowStatus.Rejected;
    }

    public void Cancel()
    {
        if (Status is ApprovalWorkflowStatus.Approved or ApprovalWorkflowStatus.Rejected or ApprovalWorkflowStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a workflow in {Status} status.");

        Status = ApprovalWorkflowStatus.Cancelled;
    }
    #endregion

    #region Step
    public void AddStep(int stepOrder, string approverRole)
    {
        Steps.Add(ApprovalStep.Create(Id, stepOrder, approverRole));
    }

    public void RemoveStep(Guid stepId)
    {
        var step = Steps.FirstOrDefault(s => s.Id == stepId)
            ?? throw ExceptionFactory.EntityNotFound<ApprovalStep>(stepId);

        Steps.Remove(step);
    }
    #endregion
}
