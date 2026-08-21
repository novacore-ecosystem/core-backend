using NovaCore.Chat.Domain.Entities.Conversations;

namespace NovaCore.Chat.Domain.Entities.Notes;

/// <summary>Persistent conversation-level information shown in a side panel - not a Message (spec section 30).</summary>
public sealed class ConversationNote : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = default!;
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public ConversationNoteType Type { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsPinned { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationNote() { }

    /// <summary>Only Conversation may construct a ConversationNote - see Conversation.AddNote.</summary>
    internal static ConversationNote Create(
        Guid conversationId,
        string title,
        string content,
        ConversationNoteType type,
        Guid createdByUserId,
        int sortOrder = 0,
        bool isPinned = false)
    {
        ValidateTitle(title);

        return new ConversationNote
        {
            Id = Guid.CreateVersion7(),
            ConversationId = conversationId,
            Title = title,
            Content = content,
            Type = type,
            SortOrder = sortOrder,
            IsPinned = isPinned,
            CreatedByUserId = createdByUserId,
        };
    }

    public void UpdateContent(string title, string content, Guid updatedByUserId)
    {
        ValidateTitle(title);

        Title = title;
        Content = content;
        UpdatedByUserId = updatedByUserId;
    }

    public void Pin() => IsPinned = true;

    public void Unpin() => IsPinned = false;

    public void ChangeSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
    }

    public static bool IsValidTitle(string? title) => !string.IsNullOrWhiteSpace(title);

    private static void ValidateTitle(string title)
    {
        if (!IsValidTitle(title))
            throw ExceptionFactory.RequiredField("Note title cannot be empty.");
    }
}
