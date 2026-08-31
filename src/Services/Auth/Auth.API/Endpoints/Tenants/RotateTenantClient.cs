using NovaCore.Auth.Application.Features.Tenants.Commands.RotateTenantClient;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

public record RotateTenantClientRequest(string? Name);

public sealed class RotateTenantClientEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Rotate Tenant Client",
        "",
        "Revokes every currently-Active client credential for this tenant and issues a new one.",
        "Returns the new public key only - the revoked key is never returned again.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/tenants/{id:guid}/client/rotate", async (
            Guid id,
            [FromBody] RotateTenantClientRequest? request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var response = await sender.Send(new RotateTenantClientCommand(id, request?.Name), ct);
            return ApiResponse<TenantClientRotationResponse>.Ok(response);
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.RotateClient)
        .WithSummary("Auth_RotateTenantClient")
        .WithDisplayName("Rotate Tenant Client API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<TenantClientRotationResponse>>();
    }
}
