using NovaCore.Auth.Domain.Entities.Tenants;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

public interface ITenantReadService
{
    Task<Tenant?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>Root Portal tenant discovery/selection (see ListTenantsQuery) - no pagination,
    /// same reasoning as INotificationChannelReadService.ListAsync: an operator-facing picker list,
    /// not a customer-facing paged catalog.</summary>
    Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken ct = default);
}
