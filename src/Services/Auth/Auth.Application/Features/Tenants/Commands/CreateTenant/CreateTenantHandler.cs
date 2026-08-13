using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.CreateTenant;

public sealed class CreateTenantHandler(
    ITenantReadService tenantReadService,
    ITenantWriteService tenantWriteService) : ICommandHandler<CreateTenantCommand, Guid>
{
    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken ct = default)
    {
        var code = TenantCode.Create(request.Code);

        if (await tenantReadService.ExistsByCodeAsync(code.Value, ct))
            throw new ConflictException($"Tenant with code ({code.Value}) already exists.");

        var tenant = Tenant.Create(code, request.Name, request.LogoUrl, request.FaviconUrl);
        await tenantWriteService.CreateAsync(tenant, ct);

        return tenant.Id;
    }
}
