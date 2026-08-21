using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.Messages;
using NovaCore.Chat.Persistence.Contexts.Messages.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.Messages.Write;

public sealed class MessageWriteService(
    IMessageRepository messageRepo,
    IUnitOfWork unitOfWork) : IMessageWriteService
{
    public async Task CreateAsync(Message message, CancellationToken ct = default)
    {
        await messageRepo.AddAsync(message, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await messageRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
