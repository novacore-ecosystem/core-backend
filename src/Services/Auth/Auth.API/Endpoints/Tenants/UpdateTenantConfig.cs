using System.Text.Json;

using NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantConfig;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

/// <summary>Merge-updates one locale's configuration - omit the `language` query parameter to
/// target the tenant-wide fallback/default configuration (see UpdateTenantConfigCommand).
/// Unspecified keys are preserved, never a wholesale replace.</summary>
public sealed class UpdateTenantConfigEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tenants/{id:guid}/config", async (
            Guid id,
            [FromQuery] string? language,
            [FromBody] JsonElement config,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new UpdateTenantConfigCommand(id, language, config), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.Manage)
        .WithSummary("Auth_UpdateTenantConfig")
        .WithDisplayName("Update Tenant Config API")
        .Produces<ApiResponse<object>>();
    }
}
