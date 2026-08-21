using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.Stickers.Repositories;

public interface IStickerRepository : IRepository<Sticker, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
