using NovaCore.Chat.Application.Abstractions.Persistence.ConversationTransferRequests;
using NovaCore.Chat.Persistence.Contexts.ConversationTransferRequests.Repositories;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationTransferRequests.Read;

public sealed class ConversationTransferRequestReadService(
    IConversationTransferRequestRepository requestRepo,
    ChatDbContext dbContext) : IConversationTransferRequestReadService
{
    public async Task<ConversationTransferRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await requestRepo.GetByIdAsync(id, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await requestRepo.ExistsByIdAsync(id, ct);
    }

    public async Task<IReadOnlyList<ConversationTransferRequest>> GetPendingForUserAsync(Guid toUserId, CancellationToken ct = default)
    {
        return await dbContext.ConversationTransferRequests
            .AsNoTracking()
            .Where(r => r.ToUserId == toUserId && r.Status == ConversationTransferStatus.Pending)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(ct);
    }
}
