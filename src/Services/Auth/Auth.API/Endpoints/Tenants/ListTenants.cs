using NovaCore.Auth.Application.Features.Tenants.Queries.ListTenants;

using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

/// <summary>
/// Tenant discovery/search for the Root Portal's Tenant Management list screen - never a
/// general-purpose tenant API. Deliberately excludes any per-tenant business data
/// (users/orders/payments/...) and any TenantClient/PublicKey field (see
/// docs/services/auth-service.md, Phase 2 "Tenant List API"). Search + pagination happen at the
/// database level (see TenantReadService.SearchAsync) - never load-all-then-filter in memory.
/// </summary>
public sealed class ListTenantsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## List Tenants",
        "",
        "Returns a paginated, searchable tenant list for the Root Portal's Tenant Management",
        "screen - discovery/selection metadata only, never credentials, public keys, or",
        "per-tenant business data.",
        "",
        "### Query Parameters",
        "- **search**: optional, matches tenant Code or Name (case-insensitive)",
        "- **page**: 1-based page number, default 1",
        "- **pageSize**: items per page, default 20",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tenants", async (
            [FromQuery] string? search,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var query = new ListTenantsQuery(
                search,
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize);
            var response = await sender.Send(query, ct);
            return ApiResponse<PaginatedResult<TenantSummaryResponse>>.Ok(response);
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.View)
        .WithSummary("Auth_ListTenants")
        .WithDisplayName("List Tenants API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<PaginatedResult<TenantSummaryResponse>>>();
    }
}
