using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Auth.Application.Features.Tenants.Queries.ListTenants;

public sealed class ListTenantsHandler(ITenantReadService tenantReadService)
    : IQueryHandler<ListTenantsQuery, PaginatedResult<TenantSummaryResponse>>
{
    public async Task<PaginatedResult<TenantSummaryResponse>> Handle(ListTenantsQuery request, CancellationToken ct = default)
    {
        var (tenants, totalCount) = await tenantReadService.SearchAsync(request.Search, request.Page, request.PageSize, ct);

        var items = tenants.Select(t => new TenantSummaryResponse(t.Id, t.Code.Value, t.Name, t.LogoUrl, t.IsActive));

        return PaginatedResult<TenantSummaryResponse>.Create(items, request.Page, request.PageSize, totalCount);
    }
}
