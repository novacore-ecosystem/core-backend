using NovaCore.Chat.Application.Abstractions.Persistence.ConversationTransferRequests;
using NovaCore.Chat.Persistence.Contexts.ConversationTransferRequests.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationTransferRequests.Read;

public sealed class ConversationTransferRequestReadService(IConversationTransferRequestRepository requestRepo) : IConversationTransferRequestReadService
{
    public async Task<ConversationTransferRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await requestRepo.GetByIdAsync(id, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await requestRepo.ExistsByIdAsync(id, ct);
    }
}
