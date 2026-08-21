using NovaCore.Chat.Application.Abstractions.Persistence.Tags;
using NovaCore.Chat.Persistence.Contexts.Tags.Repositories;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.Tags.Read;

public sealed class TagReadService(ITagRepository tagRepo, ChatDbContext dbContext) : ITagReadService
{
    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await tagRepo.GetByIdAsync(id, query => query.Include(t => t.Translations), ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await tagRepo.ExistsByIdAsync(id, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        var normalized = EntityCode.Create(code);
        return await tagRepo.ExistsAsync(t => t.Code == normalized, ct);
    }

    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Tags
            .AsNoTracking()
            .OrderBy(t => t.SortOrder)
            .ToListAsync(ct);
    }
}
