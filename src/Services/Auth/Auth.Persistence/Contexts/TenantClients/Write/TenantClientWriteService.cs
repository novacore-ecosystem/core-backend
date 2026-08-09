using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Persistence.Contexts.TenantClients.Repositories;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.TenantClients.Write;

public sealed class TenantClientWriteService(
    ITenantClientRepository repo,
    IUnitOfWork unitOfWork) : ITenantClientWriteService
{
    public async Task CreateAsync(TenantClient tenantClient, CancellationToken ct = default)
    {
        await repo.AddAsync(tenantClient, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
