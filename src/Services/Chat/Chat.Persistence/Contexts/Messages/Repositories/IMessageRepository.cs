using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.Messages.Repositories;

public interface IMessageRepository : IRepository<Message, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
