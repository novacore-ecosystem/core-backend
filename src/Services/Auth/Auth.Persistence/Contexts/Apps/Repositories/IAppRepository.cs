using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Apps.Repositories;

public interface IAppRepository : IRepository<App>
{
    /// <summary>Every App, unfiltered and without Translations - backs IAppCollectionCache's
    /// single batch query. Not expressible via the generic Get/GetMany overloads (no filter, no
    /// value to match), so it lives here as a custom method.</summary>
    Task<IReadOnlyList<App>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Database-level search + pagination for the App Management list screen - matches
    /// against Code/Name, case-insensitive. Not expressible generically (ILike + Skip/Take +
    /// count), so it lives here as a custom method.</summary>
    Task<(IReadOnlyList<App> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Every AccountId assigned to this App - queries the AccountApp join, not the App
    /// entity itself, so it isn't expressible generically.</summary>
    Task<IReadOnlyCollection<Guid>> GetAccountIdsAsync(Guid appId, CancellationToken ct = default);
}
