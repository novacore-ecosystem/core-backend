namespace NovaCore.Chat.Application.Abstractions.Persistence.Contacts;

public interface IContactWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(Contact contact, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
