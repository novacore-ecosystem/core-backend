namespace NovaCore.Chat.Application.Abstractions.Persistence.Tags;

public interface ITagReadService
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default);
}
