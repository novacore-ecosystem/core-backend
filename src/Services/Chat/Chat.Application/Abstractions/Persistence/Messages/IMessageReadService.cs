namespace NovaCore.Chat.Application.Abstractions.Persistence.Messages;

public interface IMessageReadService
{
    Task<Message?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);

    Task<long> GetLastSequenceAsync(Guid conversationId, CancellationToken ct = default);
}
