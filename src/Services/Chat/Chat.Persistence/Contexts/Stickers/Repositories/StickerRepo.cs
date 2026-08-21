using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.Stickers.Repositories;

public sealed class StickerRepo(ChatDbContext dbContext)
    : ChatBaseRepository<Sticker, Guid>(dbContext), IStickerRepository
{
}
