using NovaCore.Auth.Application.Features.Tenants.Queries.GetTenantPermissions;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

public sealed class GetTenantPermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tenants/{id:guid}/permissions", async (
            Guid id,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var response = await sender.Send(new GetTenantPermissionsQuery(id), ct);
            return ApiResponse<TenantPermissionsResponse>.Ok(response);
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.View)
        .WithSummary("Auth_GetTenantPermissions")
        .WithDisplayName("Get Tenant Permission Boundary API")
        .Produces<ApiResponse<TenantPermissionsResponse>>();
    }
}
