using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.ConversationTransferRequests.Repositories;

public interface IConversationTransferRequestRepository : IRepository<ConversationTransferRequest, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
