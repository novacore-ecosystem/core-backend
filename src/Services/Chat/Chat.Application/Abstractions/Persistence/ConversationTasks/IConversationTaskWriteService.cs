namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationTasks;

public interface IConversationTaskWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ConversationTask task, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
