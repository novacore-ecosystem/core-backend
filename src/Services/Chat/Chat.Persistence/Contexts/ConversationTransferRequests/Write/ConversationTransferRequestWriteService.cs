using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationTransferRequests;
using NovaCore.Chat.Persistence.Contexts.ConversationTransferRequests.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationTransferRequests.Write;

public sealed class ConversationTransferRequestWriteService(
    IConversationTransferRequestRepository requestRepo,
    IUnitOfWork unitOfWork) : IConversationTransferRequestWriteService
{
    public async Task CreateAsync(ConversationTransferRequest request, CancellationToken ct = default)
    {
        await requestRepo.AddAsync(request, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await requestRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
