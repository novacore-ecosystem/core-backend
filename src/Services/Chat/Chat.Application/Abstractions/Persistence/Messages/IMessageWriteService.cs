namespace NovaCore.Chat.Application.Abstractions.Persistence.Messages;

public interface IMessageWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(Message message, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
