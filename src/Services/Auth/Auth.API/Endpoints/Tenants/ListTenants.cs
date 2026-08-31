using NovaCore.Auth.Application.Features.Tenants.Queries.ListTenants;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

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
