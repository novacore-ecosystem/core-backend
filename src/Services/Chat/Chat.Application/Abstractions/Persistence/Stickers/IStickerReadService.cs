namespace NovaCore.Chat.Application.Abstractions.Persistence.Stickers;

public interface IStickerReadService
{
    Task<Sticker?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    Task<IReadOnlyList<Sticker>> GetAllAsync(CancellationToken ct = default);
}
