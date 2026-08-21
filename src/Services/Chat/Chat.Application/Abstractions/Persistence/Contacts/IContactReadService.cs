namespace NovaCore.Chat.Application.Abstractions.Persistence.Contacts;

public interface IContactReadService
{
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
}
