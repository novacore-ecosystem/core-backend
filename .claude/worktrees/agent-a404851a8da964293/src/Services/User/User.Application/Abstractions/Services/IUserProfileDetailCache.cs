namespace NovaCore.User.Application.Abstractions.Services;

/// <summary>
/// User Detail cache capability - explicit, single-purpose, called directly by handlers that want
/// it (not a decorator swapped in behind IUserReadService/IUserWriteService). Application calls
/// GetByIdAsync/GetByIdsAsync knowing it may be served from cache, and calls InvalidateAsync
/// itself at the point a write workflow has actually finished (e.g. after a transaction commits) -
/// Infrastructure never decides that timing on Application's behalf. See docs/reference/caching.md.
/// </summary>
public interface IUserProfileDetailCache
{
    Task<UserReadModel?> GetByIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Batch lookup - exactly one inner persistence round trip for whatever wasn't already cached, never a loop of single lookups.</summary>
    Task<IReadOnlyDictionary<Guid, UserReadModel>> GetByIdsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct = default);

    Task InvalidateAsync(Guid userId, CancellationToken ct = default);
}
