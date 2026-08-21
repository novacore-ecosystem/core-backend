namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationTransferRequests;

public interface IConversationTransferRequestReadService
{
    Task<ConversationTransferRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
}
