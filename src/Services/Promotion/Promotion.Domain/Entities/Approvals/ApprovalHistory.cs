namespace NovaCore.Promotion.Domain.Entities.Approvals;

/// <summary>An append-only audit trail row for an ApprovalWorkflow - CreatedAt is inherited from BaseEntity, not redeclared. Not navigated from ApprovalWorkflow, so construction is public.</summary>
public sealed class ApprovalHistory : BaseEntity<Guid>, IAuditable
{
    public Guid WorkflowId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? OperatorId { get; private set; }

    public ApprovalWorkflow Workflow { get; private set; } = default!;

    private ApprovalHistory() { }

    public static ApprovalHistory Create(Guid workflowId, string action, Guid? operatorId)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw ExceptionFactory.RequiredField("History action cannot be empty.");

        return new ApprovalHistory
        {
            Id = Guid.CreateVersion7(),
            WorkflowId = workflowId,
            Action = action,
            OperatorId = operatorId,
        };
    }
}
