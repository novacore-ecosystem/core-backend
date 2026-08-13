using NovaCore.Auth.Application.Features.Tenants.Commands.CreateTenant;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

public record CreateTenantRequest(string Code, string Name, string? LogoUrl, string? FaviconUrl);

public sealed class CreateTenantEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/tenants", async (
            [FromBody] CreateTenantRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var id = await sender.Send(
                new CreateTenantCommand(request.Code, request.Name, request.LogoUrl, request.FaviconUrl),
                ct);
            return ApiResponse<Guid>.Ok(id);
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.Manage)
        .WithSummary("Auth_CreateTenant")
        .WithDisplayName("Create Tenant API")
        .Produces<ApiResponse<Guid>>();
    }
}
