using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.DeleteTenant;

public sealed class DeleteTenantHandler(
    IUnitOfWork unitOfWork,
    ITenantWriteService tenantWriteService) : ICommandHandler<DeleteTenantCommand>
{
    public async Task Handle(DeleteTenantCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await tenantWriteService.SoftDeleteAsync(request.Id, ct);
        }, ct: ct);
    }
}
