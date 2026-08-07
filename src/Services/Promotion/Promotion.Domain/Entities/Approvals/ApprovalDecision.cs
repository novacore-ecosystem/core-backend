namespace NovaCore.Promotion.Domain.Entities.Approvals;

/// <summary>A reviewer's decision on an ApprovalStep - Decision is its own ApprovalDecisionType enum. Not navigated from ApprovalWorkflow, so construction is public.</summary>
public sealed class ApprovalDecision : BaseEntity<Guid>, IAuditable
{
    public Guid StepId { get; private set; }
    public ApprovalDecisionType Decision { get; private set; }
    public DateTime DecidedAt { get; private set; }

    public ApprovalStep Step { get; private set; } = default!;

    private ApprovalDecision() { }

    public static ApprovalDecision Create(Guid stepId, ApprovalDecisionType decision)
    {
        return new ApprovalDecision
        {
            Id = Guid.CreateVersion7(),
            StepId = stepId,
            Decision = decision,
            DecidedAt = DateTime.UtcNow,
        };
    }
}
