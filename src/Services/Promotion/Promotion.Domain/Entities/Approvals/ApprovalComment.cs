namespace NovaCore.Promotion.Domain.Entities.Approvals;

/// <summary>A reviewer comment on an ApprovalStep - CreatedAt is inherited from BaseEntity, not redeclared. Not navigated from ApprovalWorkflow, so construction is public.</summary>
public sealed class ApprovalComment : BaseEntity<Guid>, IAuditable
{
    public Guid StepId { get; private set; }
    public string Comment { get; private set; } = string.Empty;

    public ApprovalStep Step { get; private set; } = default!;

    private ApprovalComment() { }

    public static ApprovalComment Create(Guid stepId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw ExceptionFactory.RequiredField("Comment cannot be empty.");

        return new ApprovalComment
        {
            Id = Guid.CreateVersion7(),
            StepId = stepId,
            Comment = comment,
        };
    }
}
