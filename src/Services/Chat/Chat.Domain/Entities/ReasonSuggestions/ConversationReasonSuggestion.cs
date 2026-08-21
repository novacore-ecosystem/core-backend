namespace NovaCore.Chat.Domain.Entities.ReasonSuggestions;

/// <summary>
/// Admin-managed catalog of predefined conversation-start reasons, localized via
/// ConversationReasonSuggestionTranslation - same Code+Status+Translations template as Tag.
/// A suggestion is only a client-side shortcut: Conversation.Reason always stores plain text (the
/// suggestion's Text, or whatever the user typed), never a foreign key to this entity - see
/// Conversation.cs.
/// </summary>
public sealed class ConversationReasonSuggestion : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public EntityCode Code { get; private set; } = default!;
    public ConversationReasonSuggestionStatus Status { get; private set; }
    public int SortOrder { get; private set; }

    public ICollection<ConversationReasonSuggestionTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private ConversationReasonSuggestion() { }

    public static ConversationReasonSuggestion Create(EntityCode code, int sortOrder = 0)
    {
        return new ConversationReasonSuggestion
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Status = ConversationReasonSuggestionStatus.Active,
            SortOrder = sortOrder,
        };
    }
    #endregion

    #region Lifecycle
    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
    }

    public void Activate() => Status = ConversationReasonSuggestionStatus.Active;

    public void Deactivate() => Status = ConversationReasonSuggestionStatus.Inactive;
    #endregion

    #region Translations
    public void Translate(LanguageCode languageCode, string text)
    {
        ValidateText(text);

        var existing = Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            existing.UpdateText(text);
            return;
        }

        Translations.Add(ConversationReasonSuggestionTranslation.Create(Id, languageCode, text));
    }

    public static bool IsValidText(string? text) => !string.IsNullOrWhiteSpace(text);

    private static void ValidateText(string text)
    {
        if (!IsValidText(text))
            throw ExceptionFactory.RequiredField("Reason suggestion text cannot be empty.");
    }
    #endregion
}
