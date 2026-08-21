using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.Contacts.Repositories;

public interface IContactRepository : IRepository<Contact, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
