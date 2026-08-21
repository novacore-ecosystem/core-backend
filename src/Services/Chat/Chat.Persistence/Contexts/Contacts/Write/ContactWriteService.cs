using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.Contacts;
using NovaCore.Chat.Persistence.Contexts.Contacts.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.Contacts.Write;

public sealed class ContactWriteService(
    IContactRepository contactRepo,
    IUnitOfWork unitOfWork) : IContactWriteService
{
    public async Task CreateAsync(Contact contact, CancellationToken ct = default)
    {
        await contactRepo.AddAsync(contact, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await contactRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
