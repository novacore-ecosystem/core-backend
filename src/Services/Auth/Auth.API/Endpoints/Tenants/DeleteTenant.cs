using NovaCore.Auth.Application.Features.Tenants.Commands.DeleteTenant;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

/// <summary>Soft delete only - see Tenant.Delete()/ISoftDeleteEntity. Never physically removes
/// tenant data.</summary>
public sealed class DeleteTenantEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/tenants/{id:guid}", async (
            Guid id,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new DeleteTenantCommand(id), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.Manage)
        .WithSummary("Auth_DeleteTenant")
        .WithDisplayName("Delete Tenant API")
        .Produces<ApiResponse<object>>();
    }
}
