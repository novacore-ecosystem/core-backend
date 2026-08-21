using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.Polls;
using NovaCore.Chat.Persistence.Contexts.Polls.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.Polls.Write;

public sealed class PollWriteService(
    IPollRepository pollRepo,
    IUnitOfWork unitOfWork) : IPollWriteService
{
    public async Task CreateAsync(Poll poll, CancellationToken ct = default)
    {
        await pollRepo.AddAsync(poll, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await pollRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
