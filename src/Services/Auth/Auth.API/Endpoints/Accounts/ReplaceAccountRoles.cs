using NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountRoles;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints.Accounts;

public record ReplaceAccountRolesRequest(IReadOnlyCollection<Guid> RoleIds);

public sealed class ReplaceAccountRolesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/accounts/{id:guid}/roles", async (
            Guid id,
            [FromBody] ReplaceAccountRolesRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new ReplaceAccountRolesCommand(id, request.RoleIds), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Accounts")
        .RequirePermissions(Permissions.Account.Manage)
        .WithSummary("Auth_ReplaceAccountRoles")
        .WithDisplayName("Replace Account Roles API")
        .Produces<ApiResponse<object>>();
    }
}
