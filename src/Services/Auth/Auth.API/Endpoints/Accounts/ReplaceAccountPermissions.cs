using NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountPermissions;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints.Accounts;

public record ReplaceAccountPermissionsRequest(IReadOnlyCollection<string> PermissionKeys);

public sealed class ReplaceAccountPermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/accounts/{id:guid}/permissions", async (
            Guid id,
            [FromBody] ReplaceAccountPermissionsRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new ReplaceAccountPermissionsCommand(id, request.PermissionKeys), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Accounts")
        .RequirePermissions(Permissions.Account.Manage)
        .WithSummary("Auth_ReplaceAccountPermissions")
        .WithDisplayName("Replace Account Permissions API")
        .Produces<ApiResponse<object>>();
    }
}
