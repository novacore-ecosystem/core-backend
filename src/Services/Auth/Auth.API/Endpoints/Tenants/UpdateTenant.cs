using NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenant;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

public record UpdateTenantRequest(string Name, string? LogoUrl, string? FaviconUrl);

public sealed class UpdateTenantEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tenants/{id:guid}", async (
            Guid id,
            [FromBody] UpdateTenantRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new UpdateTenantCommand(id, request.Name, request.LogoUrl, request.FaviconUrl), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.Manage)
        .WithSummary("Auth_UpdateTenant")
        .WithDisplayName("Update Tenant API")
        .Produces<ApiResponse<object>>();
    }
}
