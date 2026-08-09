using NovaCore.User.Application.Features.Users.DTOs;

namespace NovaCore.User.Application.Abstractions.Persistence.Users;

public interface IUserReadService
{
    Task<UserReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Batch lookup by id, one query - used by the cache read-through and the gRPC batch RPC to avoid N+1.</summary>
    Task<IReadOnlyList<UserReadModel>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    /// <summary>Paged, ordered read of every User - used only by RebuildUserSearchIndex's batch loop.</summary>
    Task<IReadOnlyList<UserReadModel>> GetAllAsync(int skip, int take, CancellationToken ct = default);

    /// <summary>Auth's security permission projection (UserAuthorizationSnapshot - not the
    /// business-segmentation UserPermissionSnapshot, see that entity's class doc comment). Empty
    /// if no snapshot has been built yet (no authorization event received for this User).</summary>
    Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct = default);
}
