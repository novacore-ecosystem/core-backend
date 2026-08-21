using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.Stickers;
using NovaCore.Chat.Persistence.Contexts.Stickers.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.Stickers.Write;

public sealed class StickerWriteService(
    IStickerRepository stickerRepo,
    IUnitOfWork unitOfWork) : IStickerWriteService
{
    public async Task CreateAsync(Sticker sticker, CancellationToken ct = default)
    {
        await stickerRepo.AddAsync(sticker, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await stickerRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
