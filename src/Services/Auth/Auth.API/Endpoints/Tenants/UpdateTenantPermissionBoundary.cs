using NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantPermissionBoundary;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

public record UpdateTenantPermissionBoundaryRequest(bool Enabled);

public sealed class UpdateTenantPermissionBoundaryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tenants/{id:guid}/permission-boundary", async (
            Guid id,
            [FromBody] UpdateTenantPermissionBoundaryRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new UpdateTenantPermissionBoundaryCommand(id, request.Enabled), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.Manage)
        .WithSummary("Auth_UpdateTenantPermissionBoundary")
        .WithDisplayName("Update Tenant Permission Boundary Enforcement API")
        .Produces<ApiResponse<object>>();
    }
}
