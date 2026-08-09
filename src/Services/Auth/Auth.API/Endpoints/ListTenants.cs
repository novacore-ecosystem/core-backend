using NovaCore.Auth.Application.Features.Tenants.Queries.ListTenants;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints;

/// <summary>
/// Tenant discovery/selection for the Root Portal only - never a general-purpose tenant API.
/// Deliberately excludes any per-tenant business data (users/orders/payments/...) and any
/// TenantClient/PublicKey field (see docs/services/auth-service.md, Phase 2 "Tenant List API").
/// </summary>
public sealed class ListTenants : ICarterModule
{
    private readonly string[] API_DESC = [
        "## List Tenants",
        "",
        "Root-only. Returns the tenant list for the Root Portal's tenant picker - discovery/",
        "selection metadata only, never credentials, public keys, or per-tenant business data.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tenants", async (
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var response = await sender.Send(new ListTenantsQuery(), ct);
            return ApiResponse<IReadOnlyList<TenantSummaryResponse>>.Ok(response);
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Root)
        .WithSummary("Auth_ListTenants")
        .WithDisplayName("List Tenants API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<IReadOnlyList<TenantSummaryResponse>>>();
    }
}
