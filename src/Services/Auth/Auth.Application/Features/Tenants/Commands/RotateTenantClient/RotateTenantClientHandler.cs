using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Domain.Enums;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.RotateTenantClient;

/// <summary>Does not bump Tenant.Version - rotation changes which client key resolves to this
/// tenant, not the bootstrap content served once resolved (see docs/services/auth-service.md,
/// "Versioning"). A client still holding the revoked key simply fails to resolve a tenant on its
/// next request; that is a distinct problem from "reload, the content changed".</summary>
public sealed class RotateTenantClientHandler(
    ITenantReadService tenantReadService,
    ITenantClientReadService tenantClientReadService,
    ITenantClientWriteService tenantClientWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<RotateTenantClientCommand, TenantClientRotationResponse>
{
    public async Task<TenantClientRotationResponse> Handle(RotateTenantClientCommand request, CancellationToken ct = default)
    {
        var tenant = await tenantReadService.GetByIdAsync(request.TenantId, ct)
            ?? throw new NotFoundException("Tenant", request.TenantId);

        var activeClients = (await tenantClientReadService.ListByTenantAsync(request.TenantId, ct))
            .Where(c => c.Status == TenantClientStatus.Active)
            .ToList();

        var name = request.Name
            ?? (activeClients.Count == 1 ? activeClients[0].Name : $"{tenant.Name} Client");

        TenantClient newClient = null!;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            foreach (var client in activeClients)
                await tenantClientWriteService.UpdateAsync(client.Id, c => c.Revoke(RevocationReason.Superseded), ct);

            newClient = TenantClient.Create(request.TenantId, name);
            await tenantClientWriteService.CreateAsync(newClient, ct);
        }, ct: ct);

        return new TenantClientRotationResponse(newClient.Id, newClient.PublicKey.Value, newClient.ExpiresAt);
    }
}
