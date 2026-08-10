namespace NovaCore.User.Application.Abstractions.Search;

/// <summary>
/// Write-only access to the User Search index - the only place User's search sync/rebuild code
/// is allowed to mutate Elasticsearch. Wraps NovaCore.BuildingBlock.Search's generic indexer with the
/// User index name/mapping/settings. See docs/reference/search.md.
/// </summary>
public interface IUserSearchIndexer
{
    /// <summary>Idempotent - creates the index+mapping only if missing. Called on every service startup.</summary>
    Task EnsureIndexAsync(CancellationToken ct = default);

    /// <summary>Drops and recreates the index - used only by the rebuild flow.</summary>
    Task RecreateIndexAsync(CancellationToken ct = default);

    Task IndexAsync(UserSearchDocument document, CancellationToken ct = default);

    Task DeleteAsync(Guid userId, CancellationToken ct = default);

    Task BulkIndexAsync(IEnumerable<UserSearchDocument> documents, CancellationToken ct = default);
}
