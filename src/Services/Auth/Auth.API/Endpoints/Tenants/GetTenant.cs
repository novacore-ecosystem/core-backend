using NovaCore.Auth.Application.Features.Tenants.Queries.GetTenant;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

/// <summary>
/// Tenant Management editing screen - the comprehensive counterpart to ListTenants' lightweight
/// summary. Includes merged effective translations, raw per-locale config/dictionary content, and
/// non-secret client identity. Never exposes a TenantClient secret (there is none - PublicKey is
/// not a secret, see TenantClient's class doc comment).
/// </summary>
public sealed class GetTenantEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Tenant",
        "",
        "Returns the full Tenant Management editing payload: identity, branding, status, raw",
        "per-locale configuration/dictionary content (for editing), the merged effective",
        "translations per supported non-default language, and non-secret client identity.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tenants/{id:guid}", async (
            Guid id,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var response = await sender.Send(new GetTenantQuery(id), ct);
            return ApiResponse<TenantDetailResponse>.Ok(response);
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.View)
        .WithSummary("Auth_GetTenant")
        .WithDisplayName("Get Tenant API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<TenantDetailResponse>>();
    }
}
