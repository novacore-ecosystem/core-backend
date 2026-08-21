namespace NovaCore.Chat.Application.Abstractions.Persistence.Polls;

public interface IPollReadService
{
    Task<Poll?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
}
