namespace NovaCore.Content.Application.Abstractions.Persistence.ContentTypes;

public interface IContentTypeReadService
{
    Task<ContentType?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<ContentType?> GetByKeyAsync(ContentKey key, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
}
