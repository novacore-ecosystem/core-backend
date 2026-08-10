namespace NovaCore.User.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Backs the gRPC GetUsers batch RPC. Never falls back to a loop of single lookups - exactly
/// one IUserProfileDetailCache.GetByIdsAsync call, which serves from cache for whatever's
/// already there and hits the DB at most once for the rest.
/// </summary>
public sealed record GetUsersByIdsQuery(IReadOnlyList<Guid> UserIds) : IQuery<IReadOnlyDictionary<Guid, UserLookupResult>>;
