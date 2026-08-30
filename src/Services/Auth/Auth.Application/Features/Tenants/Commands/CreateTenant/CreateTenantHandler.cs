using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.CreateTenant;

public sealed class CreateTenantHandler(
    IUnitOfWork unitOfWork,
    ITenantReadService tenantReadService,
    ITenantWriteService tenantWriteService) : ICommandHandler<CreateTenantCommand, Guid>
{
    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken ct = default)
    {
        var code = TenantCode.Create(request.Code);

        if (await tenantReadService.ExistsByCodeAsync(code, ct))
            throw new ConflictException($"Tenant with code ({code.Value}) already exists.");

        Tenant newTenant = null!;
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            newTenant = Tenant.Create(
                code,
                request.Name,
                request.LogoUrl,
                request.FaviconUrl);
            await tenantWriteService.CreateAsync(newTenant, ct);
        }, ct: ct);

        return newTenant.Id;
    }
}
