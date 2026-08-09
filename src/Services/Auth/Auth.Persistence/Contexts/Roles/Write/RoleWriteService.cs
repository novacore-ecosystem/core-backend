using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Persistence.Contexts.Roles.Repositories;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Roles.Write;

public sealed class RoleWriteService(
    IRoleRepository repo,
    IUnitOfWork unitOfWork) : IRoleWriteService, IPersistenceService
{
    public async Task CreateAsync(Role role, CancellationToken ct = default)
    {
        await repo.AddAsync(role, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Guid id, Action<Role> update, CancellationToken ct = default)
    {
        await repo.UpdateAsync(r => r.Id == id, update, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateWithPermissionsAsync(Guid id, Action<Role> update, CancellationToken ct = default)
    {
        await repo.UpdateAsync(r => r.Id == id, q => q.Include(r => r.Permissions), update, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteAsync(r => r.Id == id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
