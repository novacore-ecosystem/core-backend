using Microsoft.EntityFrameworkCore;
using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Persistence.Contexts.Tenants.Repositories;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Tenants.Write;

public sealed class TenantWriteService(
    ITenantRepository repo,
    ITenantClientWriteService tenantClientWrite,
    IUnitOfWork unitOfWork) : ITenantWriteService, IPersistenceService
{
    public async Task CreateAsync(Tenant tenant, CancellationToken ct = default)
    {
        await repo.AddAsync(tenant, ct);
        await tenantClientWrite.CreateAsync(tenant.Id, tenant.Name, ct);
    }

    public async Task UpdateAsync(Guid id, Action<Tenant> update, CancellationToken ct = default)
    {
        await repo.UpdateAsync(t => t.Id == id, update, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateWithLocalesAsync(Guid id, Action<Tenant> update, CancellationToken ct = default)
    {
        await repo.UpdateAsync(t => t.Id == id, q => q.Include(t => t.Locales), update, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.UpdateAsync(
            t => t.Id == id,
            t => t.Delete(),
            ct);
    }
}
