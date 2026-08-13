using NovaCore.Auth.Application.Features.Tenants.Queries.GetTenantBootstrap;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints.Tenants;

/// <summary>
/// Pre-authentication bootstrap for the tenant's own frontend application - identified by the
/// TenantClient public key (same header Login uses), not a Root-authenticated caller. Anonymous by
/// design: this runs before a user has logged in, to render the initial app shell (branding,
/// language dictionary/config, version).
/// </summary>
public sealed class GetTenantBootstrapEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Tenant Bootstrap",
        "",
        "Pre-authentication initial-application-setup payload for the tenant identified by the",
        "client's public key - branding, the effective translation/config per supported language,",
        "and the bootstrap Version (for the future Notification Hub version check).",
        "",
        $"### Headers",
        $"- **{HeaderKeyConstant.TenantClientKey}**: TenantClient public key (required) - identifies which Tenant this bootstrap is for.",
        "",
        "### Error Responses",
        "- **401**: Invalid/unknown/revoked/expired client key",
        "- **400**: The Root client has no tenant bootstrap",
        "- **404**: Tenant not found",
        "- **409**: Tenant is disabled",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/bootstrap", async (
            [FromHeader(Name = HeaderKeyConstant.TenantClientKey)] string? clientPublicKey,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var response = await sender.Send(new GetTenantBootstrapQuery(clientPublicKey?.Trim() ?? string.Empty), ct);
            return ApiResponse<TenantBootstrapResponse>.Ok(response);
        })
        .WithTags("Tenants")
        .AllowAnonymous()
        .WithSummary("Auth_GetTenantBootstrap")
        .WithDisplayName("Tenant Bootstrap API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<TenantBootstrapResponse>>();
    }
}
