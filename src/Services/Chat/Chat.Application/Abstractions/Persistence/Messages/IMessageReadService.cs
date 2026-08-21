namespace NovaCore.Chat.Application.Abstractions.Persistence.Messages;

public interface IMessageReadService
{
    Task<Message?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);

    Task<long> GetLastSequenceAsync(Guid conversationId, CancellationToken ct = default);

    /// <summary>Messages after the given sequence, ascending, capped at limit - the gap-recovery read (see ChatHub.RecoverMessages).</summary>
    Task<IReadOnlyList<Message>> GetSinceSequenceAsync(Guid conversationId, long afterSequence, int limit, CancellationToken ct = default);
}
