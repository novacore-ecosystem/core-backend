using NovaCore.Auth.Application.Features.Registrations.Commands.ReplaceRegistrationDefaultPermissions;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Registrations;

public record ReplaceRegistrationDefaultPermissionsRequest(IReadOnlyCollection<string> PermissionKeys);

public sealed class ReplaceRegistrationDefaultPermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/apps/{appId:guid}/registration-defaults/permissions", async (
            Guid appId,
            [FromBody] ReplaceRegistrationDefaultPermissionsRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new ReplaceRegistrationDefaultPermissionsCommand(appId, request.PermissionKeys), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Registrations")
        .RequirePermissions(Permissions.RegistrationDefaults.Manage)
        .WithSummary("Auth_ReplaceRegistrationDefaultPermissions")
        .WithDisplayName("Replace Registration Default Permissions API")
        .Produces<ApiResponse<object>>();
    }
}
