using NovaCore.Auth.Application.Features.Tenants.Commands.AssignTenantPermissions;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

public record AssignTenantPermissionsRequest(IReadOnlyCollection<string> Grant, IReadOnlyCollection<string> Revoke);

/// <summary>ROOT's own capability for imposing a tenant's permission boundary - distinct from
/// UpdateRolePermissions/ReplaceAccountPermissions, which mutate what a tenant's Role/Account
/// actually holds within that boundary.</summary>
public sealed class AssignTenantPermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tenants/{id:guid}/permissions", async (
            Guid id,
            [FromBody] AssignTenantPermissionsRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new AssignTenantPermissionsCommand(id, request.Grant, request.Revoke), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.Manage)
        .WithSummary("Auth_AssignTenantPermissions")
        .WithDisplayName("Assign Tenant Permission Boundary API")
        .Produces<ApiResponse<object>>();
    }
}
