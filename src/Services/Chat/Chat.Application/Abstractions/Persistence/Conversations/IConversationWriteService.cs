namespace NovaCore.Chat.Application.Abstractions.Persistence.Conversations;

public interface IConversationWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(Conversation conversation, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CloseAsync(Guid id, CancellationToken ct = default);
}
