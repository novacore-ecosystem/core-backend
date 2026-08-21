namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationResponsibilityHistories;

public interface IConversationResponsibilityHistoryWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ConversationResponsibilityHistory history, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
