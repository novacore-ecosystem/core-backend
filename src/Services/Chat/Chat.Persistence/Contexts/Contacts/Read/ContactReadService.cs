using NovaCore.Chat.Application.Abstractions.Persistence.Contacts;
using NovaCore.Chat.Persistence.Contexts.Contacts.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.Contacts.Read;

public sealed class ContactReadService(IContactRepository contactRepo) : IContactReadService
{
    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await contactRepo.GetByIdAsync(id, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await contactRepo.ExistsByIdAsync(id, ct);
    }
}
