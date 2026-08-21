namespace NovaCore.Chat.Domain.Entities.Assignments;

/// <summary>Current/previous CS handling ownership for a conversation - independently constructible by ConversationId (not owned by Conversation), needs its own Id since a conversation can accumulate multiple assignment cycles over its life.</summary>
public sealed class ConversationAssignment : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Guid AssigneeUserId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }
    public ConversationAssignmentStatus Status { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationAssignment() { }

    public static ConversationAssignment Create(Guid conversationId, Guid assigneeUserId, ChatMetadata? metadata = null)
    {
        return new ConversationAssignment
        {
            Id = Guid.CreateVersion7(),
            ConversationId = conversationId,
            AssigneeUserId = assigneeUserId,
            AssignedAt = DateTime.UtcNow,
            Status = ConversationAssignmentStatus.Active,
            Metadata = metadata,
        };
    }

    public void Release()
    {
        if (Status != ConversationAssignmentStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot release an assignment in {Status} status.");

        Status = ConversationAssignmentStatus.Released;
        ReleasedAt = DateTime.UtcNow;
    }
}
