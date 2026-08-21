using NovaCore.Chat.Application.Abstractions.Persistence.Polls;
using NovaCore.Chat.Persistence.Contexts.Polls.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.Polls.Read;

public sealed class PollReadService(IPollRepository pollRepo) : IPollReadService
{
    public async Task<Poll?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await pollRepo.GetByIdAsync(id, query => query.Include(p => p.Options), ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await pollRepo.ExistsByIdAsync(id, ct);
    }
}
