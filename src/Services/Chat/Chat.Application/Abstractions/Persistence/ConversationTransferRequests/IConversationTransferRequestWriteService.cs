namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationTransferRequests;

public interface IConversationTransferRequestWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ConversationTransferRequest request, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
