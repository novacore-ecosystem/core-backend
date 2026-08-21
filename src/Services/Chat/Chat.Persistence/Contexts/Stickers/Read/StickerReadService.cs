using NovaCore.Chat.Application.Abstractions.Persistence.Stickers;
using NovaCore.Chat.Persistence.Contexts.Stickers.Repositories;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.Stickers.Read;

public sealed class StickerReadService(IStickerRepository stickerRepo, ChatDbContext dbContext) : IStickerReadService
{
    public async Task<Sticker?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await stickerRepo.GetByIdAsync(id, query => query.Include(s => s.Translations), ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await stickerRepo.ExistsByIdAsync(id, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        var normalized = EntityCode.Create(code);
        return await stickerRepo.ExistsAsync(s => s.Code == normalized, ct);
    }

    public async Task<IReadOnlyList<Sticker>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Stickers
            .AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .ToListAsync(ct);
    }
}
