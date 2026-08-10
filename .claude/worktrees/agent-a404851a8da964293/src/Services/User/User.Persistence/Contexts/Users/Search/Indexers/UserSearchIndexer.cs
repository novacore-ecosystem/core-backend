using NovaCore.BuildingBlock.Search.Abstractions;

using NovaCore.User.Application.Abstractions.Search;
using NovaCore.User.Persistence.Contexts.Users.Search.Mapping;

namespace NovaCore.User.Persistence.Contexts.Users.Search.Indexers;

/// <summary>
/// IUserSearchIndexer impl - fixes the User index name/mapping/settings on top of
/// NovaCore.BuildingBlock.Search's generic, reusable IElasticsearchIndexer&lt;&gt;. The name passed
/// through here is an ES alias, not a concrete index - EnsureIndexAsync/RecreateIndexAsync
/// manage the versioned index + alias swap underneath; this class stays unaware of that detail.
/// </summary>
public sealed class UserSearchIndexer(IElasticsearchIndexer<UserSearchDocument> indexer) : IUserSearchIndexer
{
    public Task EnsureIndexAsync(CancellationToken ct = default) =>
        indexer.EnsureIndexAsync(UserSearchIndexNames.Default, UserSearchIndexMapping.Configure, UserSearchIndexMapping.ConfigureSettings, ct);

    public Task RecreateIndexAsync(CancellationToken ct = default) =>
        indexer.RecreateIndexAsync(UserSearchIndexNames.Default, UserSearchIndexMapping.Configure, UserSearchIndexMapping.ConfigureSettings, ct);

    public Task IndexAsync(UserSearchDocument document, CancellationToken ct = default) =>
        indexer.IndexAsync(UserSearchIndexNames.Default, document.UserId.ToString(), document, ct);

    public Task DeleteAsync(Guid userId, CancellationToken ct = default) =>
        indexer.DeleteAsync(UserSearchIndexNames.Default, userId.ToString(), ct);

    public Task BulkIndexAsync(IEnumerable<UserSearchDocument> documents, CancellationToken ct = default) =>
        indexer.BulkIndexAsync(
            UserSearchIndexNames.Default,
            documents.Select(d => (d.UserId.ToString(), d)),
            ct);
}
