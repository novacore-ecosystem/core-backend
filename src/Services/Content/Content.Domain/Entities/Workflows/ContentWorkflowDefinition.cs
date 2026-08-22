namespace NovaCore.Content.Domain.Entities.Workflows;

/// <summary>
/// Aggregate root describing a reusable content approval/publication workflow - a set of States
/// and the Transitions allowed between them. Content items don't inherit a workflow directly;
/// a ContentWorkflowInstance (owned by Content) tracks one Content's progress through one
/// ContentWorkflowDefinition over time.
/// </summary>
public sealed class ContentWorkflowDefinition : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public ContentKey Key { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public WorkflowStatus Status { get; private set; }

    public ICollection<ContentWorkflowState> States { get; private set; } = [];
    public ICollection<ContentWorkflowTransition> Transitions { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentWorkflowDefinition() { }

    public static ContentWorkflowDefinition Create(
        ContentKey key,
        string name,
        string description,
        WorkflowStatus status = WorkflowStatus.Active)
    {
        ValidateName(name);

        return new ContentWorkflowDefinition
        {
            Id = Guid.CreateVersion7(),
            Key = key,
            Name = name,
            Description = description,
            Status = status,
        };
    }

    // ============================================================================
    // States & Transitions
    // Manages the owned State/Transition graph. A Transition may only reference States
    // that already belong to this definition, and at most one State may be Initial.
    // ============================================================================

    #region States & Transitions

    public ContentWorkflowState AddState(
        ContentKey key,
        string name,
        string description,
        bool isInitial = false,
        bool isFinal = false,
        int displayOrder = 0)
    {
        if (States.Any(s => s.Key == key))
            throw ExceptionFactory.Duplicate($"A workflow state with key '{key}' already exists on this definition.");

        if (isInitial && States.Any(s => s.IsInitial))
            throw ExceptionFactory.InvalidState("A workflow definition can only have one initial state.");

        var state = ContentWorkflowState.Create(Id, key, name, description, isInitial, isFinal, displayOrder);
        States.Add(state);

        return state;
    }

    public ContentWorkflowTransition AddTransition(
        ContentKey key,
        string name,
        string description,
        Guid fromStateId,
        Guid toStateId)
    {
        var fromState = States.FirstOrDefault(s => s.Id == fromStateId)
            ?? throw ExceptionFactory.EntityNotFound<ContentWorkflowState>(fromStateId);
        var toState = States.FirstOrDefault(s => s.Id == toStateId)
            ?? throw ExceptionFactory.EntityNotFound<ContentWorkflowState>(toStateId);

        if (Transitions.Any(t => t.FromStateId == fromStateId && t.ToStateId == toStateId))
            throw ExceptionFactory.Duplicate("A transition between these two states already exists.");

        var transition = ContentWorkflowTransition.Create(Id, key, name, description, fromState.Id, toState.Id);
        Transitions.Add(transition);

        return transition;
    }

    /// <summary>Whether a direct transition from one state to another is allowed by this definition.</summary>
    public bool CanTransition(Guid fromStateId, Guid toStateId)
        => Transitions.Any(t => t.FromStateId == fromStateId && t.ToStateId == toStateId);

    #endregion

    // ============================================================================
    // Details & lifecycle
    // ============================================================================

    #region Details & lifecycle

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public void Activate()
    {
        Status = WorkflowStatus.Active;
    }

    public void Deactivate()
    {
        Status = WorkflowStatus.Inactive;
    }

    public void Archive()
    {
        Status = WorkflowStatus.Archived;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Workflow definition name cannot be empty.");
    }

    #endregion
}
