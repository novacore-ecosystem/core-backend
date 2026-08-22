namespace NovaCore.Content.Domain.Entities.Workflows;

/// <summary>One allowed edge between two ContentWorkflowStates in a ContentWorkflowDefinition.</summary>
public sealed class ContentWorkflowTransition : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid WorkflowDefinitionId { get; private set; }
    public ContentWorkflowDefinition WorkflowDefinition { get; private set; } = default!;
    public Guid FromStateId { get; private set; }
    public ContentWorkflowState FromState { get; private set; } = default!;
    public Guid ToStateId { get; private set; }
    public ContentWorkflowState ToState { get; private set; } = default!;
    public ContentKey Key { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentWorkflowTransition() { }

    internal static ContentWorkflowTransition Create(
        Guid workflowDefinitionId,
        ContentKey key,
        string name,
        string description,
        Guid fromStateId,
        Guid toStateId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Workflow transition name cannot be empty.");

        if (fromStateId == toStateId)
            throw ExceptionFactory.InvalidState("A workflow transition cannot start and end on the same state.");

        return new ContentWorkflowTransition
        {
            Id = Guid.CreateVersion7(),
            WorkflowDefinitionId = workflowDefinitionId,
            Key = key,
            Name = name,
            Description = description,
            FromStateId = fromStateId,
            ToStateId = toStateId,
        };
    }
}
