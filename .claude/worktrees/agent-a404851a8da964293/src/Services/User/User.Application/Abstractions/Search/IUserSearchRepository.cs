namespace NovaCore.User.Application.Abstractions.Search;

/// <summary>
/// Query-only access to the User Search index. Deliberately exposes no Add/Update/Delete -
/// indexing is IUserSearchIndexer's responsibility, never this repository's. See
/// docs/reference/search.md.
/// </summary>
public interface IUserSearchRepository
{
    Task<(IReadOnlyList<UserSearchDocument> Items, long TotalCount)> SearchAsync(
        UserSearchCriteria criteria, CancellationToken ct = default);
}
