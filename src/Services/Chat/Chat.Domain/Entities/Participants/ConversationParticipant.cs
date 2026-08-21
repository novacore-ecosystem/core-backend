using NovaCore.Chat.Domain.Entities.Conversations;

namespace NovaCore.Chat.Domain.Entities.Participants;

/// <summary>
/// Current membership row for one user in one conversation - composite key (ConversationId,
/// UserId) per spec section 13, not a surrogate Id, since this holds current state (Role/Status/
/// LastReadSequence/...) rather than a history of every membership event.
/// </summary>
public sealed class ConversationParticipant : BaseEntity, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = default!;
    public Guid UserId { get; private set; }
    public ParticipantRole Role { get; private set; }
    public ConversationParticipantStatus Status { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
    public bool IsMuted { get; private set; }
    public long LastReadSequence { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationParticipant() { }

    /// <summary>Only Conversation may construct a ConversationParticipant - see Conversation.AddParticipant.</summary>
    internal static ConversationParticipant Create(
        Guid conversationId,
        Guid userId,
        ParticipantRole role,
        ChatMetadata? metadata = null)
    {
        return new ConversationParticipant
        {
            ConversationId = conversationId,
            UserId = userId,
            Role = role,
            Status = ConversationParticipantStatus.Active,
            JoinedAt = DateTime.UtcNow,
            LastReadSequence = 0,
            Metadata = metadata,
        };
    }

    internal void ChangeRole(ParticipantRole role)
    {
        Role = role;
    }

    internal void Mute()
    {
        IsMuted = true;
    }

    internal void Unmute()
    {
        IsMuted = false;
    }

    internal void UpdateLastRead(long sequence)
    {
        if (sequence > LastReadSequence)
            LastReadSequence = sequence;
    }

    /// <summary>Voluntary departure - see Remove for admin-initiated removal.</summary>
    internal void Leave()
    {
        Status = ConversationParticipantStatus.Left;
        LeftAt = DateTime.UtcNow;
    }

    internal void Remove()
    {
        Status = ConversationParticipantStatus.Removed;
        LeftAt = DateTime.UtcNow;
    }
}
