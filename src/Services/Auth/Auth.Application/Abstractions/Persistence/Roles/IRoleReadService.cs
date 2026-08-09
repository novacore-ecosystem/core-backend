using NovaCore.Auth.Domain.Entities.Roles;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Roles;

public interface IRoleReadService
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct = default);
}
