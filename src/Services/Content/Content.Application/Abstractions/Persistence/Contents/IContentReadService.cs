namespace NovaCore.Content.Application.Abstractions.Persistence.Contents;

public interface IContentReadService
{
    Task<ContentEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<ContentEntity?> GetBySlugAsync(ContentSlug slug, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsBySlugAsync(ContentSlug slug, CancellationToken ct = default);
}
