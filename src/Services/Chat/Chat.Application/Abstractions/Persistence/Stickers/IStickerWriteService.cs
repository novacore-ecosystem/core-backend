namespace NovaCore.Chat.Application.Abstractions.Persistence.Stickers;

public interface IStickerWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(Sticker sticker, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
