using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.DisableTenant;

public sealed class DisableTenantHandler(
    IUnitOfWork unitOfWork,
    ITenantWriteService tenantWriteService) : ICommandHandler<DisableTenantCommand>
{
    public async Task Handle(DisableTenantCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await tenantWriteService.DisableAsync(request.Id, ct);
        }, ct: ct);
    }
}
