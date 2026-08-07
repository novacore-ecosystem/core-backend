namespace NovaCore.Promotion.Domain.Entities.Approvals;

/// <summary>A user assigned to review an ApprovalStep - not navigated from ApprovalWorkflow, so construction is public.</summary>
public sealed class ApprovalAssignment : BaseEntity<Guid>, IAuditable
{
    public Guid StepId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    public ApprovalStep Step { get; private set; } = default!;

    private ApprovalAssignment() { }

    public static ApprovalAssignment Create(Guid stepId, Guid userId)
    {
        return new ApprovalAssignment
        {
            Id = Guid.CreateVersion7(),
            StepId = stepId,
            UserId = userId,
            AssignedAt = DateTime.UtcNow,
        };
    }
}
