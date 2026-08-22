namespace NovaCore.Content.Domain.Entities.Workflows;

/// <summary>One node in a ContentWorkflowDefinition's state graph.</summary>
public sealed class ContentWorkflowState : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid WorkflowDefinitionId { get; private set; }
    public ContentWorkflowDefinition WorkflowDefinition { get; private set; } = default!;
    public ContentKey Key { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsInitial { get; private set; }
    public bool IsFinal { get; private set; }
    public int DisplayOrder { get; private set; }

    public ICollection<ContentWorkflowTransition> OutgoingTransitions { get; private set; } = [];
    public ICollection<ContentWorkflowTransition> IncomingTransitions { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentWorkflowState() { }

    internal static ContentWorkflowState Create(
        Guid workflowDefinitionId,
        ContentKey key,
        string name,
        string description,
        bool isInitial,
        bool isFinal,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Workflow state name cannot be empty.");

        if (isInitial && isFinal)
            throw ExceptionFactory.InvalidState("A workflow state cannot be both initial and final.");

        return new ContentWorkflowState
        {
            Id = Guid.CreateVersion7(),
            WorkflowDefinitionId = workflowDefinitionId,
            Key = key,
            Name = name,
            Description = description,
            IsInitial = isInitial,
            IsFinal = isFinal,
            DisplayOrder = displayOrder,
        };
    }
}
