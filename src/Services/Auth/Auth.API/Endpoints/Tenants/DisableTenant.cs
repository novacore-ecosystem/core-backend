using NovaCore.Auth.Application.Features.Tenants.Commands.DisableTenant;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

public sealed class DisableTenantEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/tenants/{id:guid}/disable", async (
            Guid id,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new DisableTenantCommand(id), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.Manage)
        .WithSummary("Auth_DisableTenant")
        .WithDisplayName("Disable Tenant API")
        .Produces<ApiResponse<object>>();
    }
}
