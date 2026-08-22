namespace NovaCore.Content.Domain.Entities.Contents;

/// <summary>
/// Tracks one Content's progress through one ContentWorkflowDefinition. Deliberately distinct
/// from PublicationStatus - a Content can be mid-review-workflow while still showing its last
/// published version live, and vice versa.
/// </summary>
public sealed class ContentWorkflowInstance : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid ContentId { get; private set; }
    public Content Content { get; private set; } = default!;
    public Guid WorkflowDefinitionId { get; private set; }
    public ContentWorkflowDefinition WorkflowDefinition { get; private set; } = default!;
    public string CurrentState { get; private set; } = string.Empty;
    public Guid StartedBy { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentWorkflowInstance() { }

    internal static ContentWorkflowInstance Create(
        Guid contentId,
        Guid workflowDefinitionId,
        string initialState,
        Guid startedBy)
    {
        if (string.IsNullOrWhiteSpace(initialState))
            throw ExceptionFactory.RequiredField("Workflow instance initial state cannot be empty.");

        return new ContentWorkflowInstance
        {
            Id = Guid.CreateVersion7(),
            ContentId = contentId,
            WorkflowDefinitionId = workflowDefinitionId,
            CurrentState = initialState,
            StartedBy = startedBy,
            StartedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Moves this instance to a new state. Whether the transition is actually allowed by the
    /// owning ContentWorkflowDefinition's graph is validated by the Application-layer handler
    /// (it needs ContentWorkflowDefinition.CanTransition, a cross-aggregate lookup this instance
    /// cannot perform on its own) before calling this method.
    /// </summary>
    internal void TransitionTo(string newState)
    {
        if (CompletedAt.HasValue)
            throw ExceptionFactory.InvalidState("Cannot transition a completed workflow instance.");

        if (string.IsNullOrWhiteSpace(newState))
            throw ExceptionFactory.RequiredField("Workflow instance state cannot be empty.");

        CurrentState = newState;
    }

    internal void Complete()
    {
        if (CompletedAt.HasValue)
            throw ExceptionFactory.InvalidState("Workflow instance is already completed.");

        CompletedAt = DateTime.UtcNow;
    }
}
