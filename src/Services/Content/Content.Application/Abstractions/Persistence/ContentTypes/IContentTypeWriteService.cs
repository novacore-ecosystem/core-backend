namespace NovaCore.Content.Application.Abstractions.Persistence.ContentTypes;

public interface IContentTypeWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - a brand-new ContentType is a single-aggregate write.</summary>
    Task CreateAsync(ContentType contentType, CancellationToken ct = default);
}
