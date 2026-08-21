using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.Contacts.Repositories;

public sealed class ContactRepo(ChatDbContext dbContext)
    : ChatBaseRepository<Contact, Guid>(dbContext), IContactRepository
{
}
