using NovaCore.Auth.Application.Features.Registrations.Commands.ReplaceRegistrationDefaultRoles;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Registrations;

public record ReplaceRegistrationDefaultRolesRequest(IReadOnlyCollection<Guid> RoleIds);

public sealed class ReplaceRegistrationDefaultRolesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/apps/{appId:guid}/registration-defaults/roles", async (
            Guid appId,
            [FromBody] ReplaceRegistrationDefaultRolesRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new ReplaceRegistrationDefaultRolesCommand(appId, request.RoleIds), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Registrations")
        .RequirePermissions(Permissions.RegistrationDefaults.Manage)
        .WithSummary("Auth_ReplaceRegistrationDefaultRoles")
        .WithDisplayName("Replace Registration Default Roles API")
        .Produces<ApiResponse<object>>();
    }
}
