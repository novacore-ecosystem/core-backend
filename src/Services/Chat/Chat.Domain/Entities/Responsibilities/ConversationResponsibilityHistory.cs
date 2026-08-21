namespace NovaCore.Chat.Domain.Entities.Responsibilities;

/// <summary>
/// Tracks who was responsible for handling a conversation and why (CS handover/reporting) - not a
/// generic audit log (spec section 18 is explicit about the distinction). Independently
/// constructible by ConversationId.
/// </summary>
public sealed class ConversationResponsibilityHistory : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Guid UserId { get; private set; }
    public ResponsibilityType ResponsibilityType { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public ResponsibilitySource Source { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationResponsibilityHistory() { }

    public static ConversationResponsibilityHistory Create(
        Guid conversationId,
        Guid userId,
        ResponsibilityType responsibilityType,
        ResponsibilitySource source,
        ChatMetadata? metadata = null)
    {
        return new ConversationResponsibilityHistory
        {
            Id = Guid.CreateVersion7(),
            ConversationId = conversationId,
            UserId = userId,
            ResponsibilityType = responsibilityType,
            Source = source,
            StartedAt = DateTime.UtcNow,
            Metadata = metadata,
        };
    }

    public void End()
    {
        if (EndedAt is not null)
            throw ExceptionFactory.InvalidState("Responsibility period has already ended.");

        EndedAt = DateTime.UtcNow;
    }
}
