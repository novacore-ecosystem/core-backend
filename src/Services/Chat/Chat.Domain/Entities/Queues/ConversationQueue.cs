namespace NovaCore.Chat.Domain.Entities.Queues;

/// <summary>A business queue of conversations waiting for CS assignment (WCM-style) - not messaging infrastructure. Atomic claim/locking behavior belongs to Application/Infrastructure.</summary>
public sealed class ConversationQueue : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ConversationQueueStatus Status { get; private set; }
    public int Priority { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private ConversationQueue() { }

    public static ConversationQueue Create(
        EntityCode code,
        string name,
        string? description = null,
        int priority = 0,
        ChatMetadata? metadata = null)
    {
        ValidateName(name);

        return new ConversationQueue
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Description = description,
            Status = ConversationQueueStatus.Active,
            Priority = priority,
            Metadata = metadata,
        };
    }
    #endregion

    #region Details & lifecycle
    public void UpdateDetails(string name, string? description, int priority)
    {
        ValidateName(name);

        Name = name;
        Description = description;
        Priority = priority;
    }

    public void Activate() => Status = ConversationQueueStatus.Active;

    public void Deactivate() => Status = ConversationQueueStatus.Inactive;

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Queue name cannot be empty.");
    }
    #endregion
}
