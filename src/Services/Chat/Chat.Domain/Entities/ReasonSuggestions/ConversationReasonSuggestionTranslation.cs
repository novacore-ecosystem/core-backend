namespace NovaCore.Chat.Domain.Entities.ReasonSuggestions;

/// <summary>Per-language phrase for a ConversationReasonSuggestion. Id doubles as the owning suggestion's Id - composite key (Id, LanguageCode).</summary>
public sealed class ConversationReasonSuggestionTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public ConversationReasonSuggestion ConversationReasonSuggestion { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Text { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationReasonSuggestionTranslation() { }

    /// <summary>Only ConversationReasonSuggestion may construct this - see ConversationReasonSuggestion.Translate.</summary>
    internal static ConversationReasonSuggestionTranslation Create(Guid suggestionId, LanguageCode languageCode, string text)
    {
        return new ConversationReasonSuggestionTranslation
        {
            Id = suggestionId,
            LanguageCode = languageCode,
            Text = text,
        };
    }

    internal void UpdateText(string text)
    {
        Text = text;
    }
}
