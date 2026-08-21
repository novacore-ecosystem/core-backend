using NovaCore.Chat.Domain.Entities.Contacts;
using NovaCore.Chat.Domain.Entities.Notes;
using NovaCore.Chat.Domain.Entities.Participants;
using NovaCore.Chat.Domain.Entities.Pins;
using NovaCore.Chat.Domain.Entities.Tags;

namespace NovaCore.Chat.Domain.Entities.Conversations;

/// <summary>
/// Root aggregate of the Chat domain. Owns its bounded child collections (Participants,
/// TagMappings, Notes, PinnedMessages) directly. Message/ConversationAssignment/
/// ConversationTransferRequest/ConversationResponsibilityHistory/ConversationReadState/
/// ConversationNotificationPreference/ConversationQueueItem are all independently constructible
/// and related by ConversationId only (no back-collection here) - same pattern
/// Promotion.Domain uses for PromotionRuleGroup/PromotionPriority/PromotionExclusion, applied here
/// because those collections are unbounded (Message) or not meaningfully loaded through the
/// aggregate (the rest).
/// </summary>
public sealed class Conversation : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public ConversationType Type { get; private set; }
    public ConversationLifecycle Lifecycle { get; private set; }
    public ConversationStatus Status { get; private set; }
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public string? Avatar { get; private set; }
    public string? Reason { get; private set; }
    public ConversationPriority Priority { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public long LastMessageSequence { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public ICollection<ConversationContact> ContactMappings { get; private set; } = [];
    public ICollection<ConversationParticipant> Participants { get; private set; } = [];
    public ICollection<ConversationTag> TagMappings { get; private set; } = [];
    public ICollection<ConversationNote> Notes { get; private set; } = [];
    public ICollection<ConversationPinnedMessage> PinnedMessages { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Conversation() { }

    public static Conversation Create(
        ConversationType type,
        ConversationLifecycle lifecycle,
        string? title = null,
        string? description = null,
        string? avatar = null,
        string? reason = null,
        ConversationPriority priority = ConversationPriority.Normal,
        ConversationStatus status = ConversationStatus.Open,
        ChatMetadata? metadata = null)
    {
        var now = DateTime.UtcNow;

        return new Conversation
        {
            Id = Guid.CreateVersion7(),
            Type = type,
            Lifecycle = lifecycle,
            Status = status,
            Title = title,
            Description = description,
            Avatar = avatar,
            Reason = reason,
            Priority = priority,
            LastActivityAt = now,
            LastMessageSequence = 0,
            Metadata = metadata,
        };
    }
    #endregion

    #region Details & lifecycle
    public void UpdateDetails(string? title, string? description, string? avatar)
    {
        Title = title;
        Description = description;
        Avatar = avatar;
    }

    public void ChangePriority(ConversationPriority priority)
    {
        Priority = priority;
    }

    public void RecordActivity(long messageSequence)
    {
        LastActivityAt = DateTime.UtcNow;
        LastMessageSequence = messageSequence;
    }

    public void UpdateMetadata(ChatMetadata metadata)
    {
        Metadata = metadata;
    }
    #endregion

    #region Status
    public void Enqueue()
    {
        Status = ConversationStatus.Queued;
    }

    public void Open()
    {
        if (Status == ConversationStatus.Closed)
            throw ExceptionFactory.InvalidStatus("Cannot open a closed conversation.");

        Status = ConversationStatus.Open;
    }

    public void MarkPending()
    {
        if (Status != ConversationStatus.Open)
            throw ExceptionFactory.InvalidStatus($"Cannot mark a conversation pending from {Status} status.");

        Status = ConversationStatus.Pending;
    }

    public void Close()
    {
        if (Status == ConversationStatus.Closed)
            throw ExceptionFactory.InvalidStatus("Conversation is already closed.");

        Status = ConversationStatus.Closed;
        ClosedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        if (Status != ConversationStatus.Closed)
            throw ExceptionFactory.InvalidStatus("Only a closed conversation can be reopened.");

        Status = ConversationStatus.Open;
        ClosedAt = null;
    }
    #endregion

    #region Contact
    /// <summary>A conversation has at most one business contact identity - guards against a second AssignContact call rather than allowing many.</summary>
    public void AssignContact(Guid contactId)
    {
        if (ContactMappings.Count > 0)
            throw ExceptionFactory.InvalidState("Conversation already has a contact assigned.");

        ContactMappings.Add(ConversationContact.Create(Id, contactId));
    }
    #endregion

    #region Participants
    public void AddParticipant(Guid userId, ParticipantRole role, ChatMetadata? metadata = null)
    {
        if (Participants.Any(p => p.UserId == userId))
            throw ExceptionFactory.Duplicate("User is already a participant of this conversation.");

        Participants.Add(ConversationParticipant.Create(Id, userId, role, metadata));
    }

    public void RemoveParticipant(Guid userId)
    {
        var participant = Participants.FirstOrDefault(p => p.UserId == userId)
            ?? throw ExceptionFactory.EntityNotFound<ConversationParticipant>(userId);

        participant.Remove();
    }

    public void MuteParticipant(Guid userId)
    {
        var participant = Participants.FirstOrDefault(p => p.UserId == userId)
            ?? throw ExceptionFactory.EntityNotFound<ConversationParticipant>(userId);

        participant.Mute();
    }

    public void UnmuteParticipant(Guid userId)
    {
        var participant = Participants.FirstOrDefault(p => p.UserId == userId)
            ?? throw ExceptionFactory.EntityNotFound<ConversationParticipant>(userId);

        participant.Unmute();
    }
    #endregion

    #region Tags
    public void AssignTag(Guid tagId)
    {
        if (TagMappings.Any(t => t.TagId == tagId))
            return;

        TagMappings.Add(ConversationTag.Create(Id, tagId));
    }

    public void RemoveTag(Guid tagId)
    {
        var mapping = TagMappings.FirstOrDefault(t => t.TagId == tagId);
        if (mapping is null)
            return;

        TagMappings.Remove(mapping);
    }
    #endregion

    #region Notes
    public void AddNote(
        string title,
        string content,
        ConversationNoteType type,
        Guid createdByUserId,
        int sortOrder = 0,
        bool isPinned = false)
    {
        Notes.Add(ConversationNote.Create(Id, title, content, type, createdByUserId, sortOrder, isPinned));
    }

    public void RemoveNote(Guid noteId)
    {
        var note = Notes.FirstOrDefault(n => n.Id == noteId)
            ?? throw ExceptionFactory.EntityNotFound<ConversationNote>(noteId);

        Notes.Remove(note);
    }
    #endregion

    #region Pinned messages
    public void PinMessage(Guid messageId, Guid pinnedByUserId)
    {
        if (PinnedMessages.Any(p => p.MessageId == messageId))
            return;

        PinnedMessages.Add(ConversationPinnedMessage.Create(Id, messageId, pinnedByUserId));
    }

    public void UnpinMessage(Guid messageId)
    {
        var pinned = PinnedMessages.FirstOrDefault(p => p.MessageId == messageId);
        if (pinned is null)
            return;

        PinnedMessages.Remove(pinned);
    }
    #endregion
}
