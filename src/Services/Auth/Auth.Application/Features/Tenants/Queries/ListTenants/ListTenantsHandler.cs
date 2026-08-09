using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

namespace NovaCore.Auth.Application.Features.Tenants.Queries.ListTenants;

public sealed class ListTenantsHandler(ITenantReadService tenantReadService)
    : IQueryHandler<ListTenantsQuery, IReadOnlyList<TenantSummaryResponse>>
{
    public async Task<IReadOnlyList<TenantSummaryResponse>> Handle(ListTenantsQuery request, CancellationToken ct = default)
    {
        var tenants = await tenantReadService.ListAsync(ct);

        return [.. tenants.Select(t => new TenantSummaryResponse(t.Id, t.Code.Value, t.Name, t.LogoUrl, t.IsActive))];
    }
}
