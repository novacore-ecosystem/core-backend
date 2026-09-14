using NovaCore.Auth.Application.Features.Accounts.Commands.RemoveAccountApp;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Accounts;

public sealed class RemoveAccountAppEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/accounts/{accountId:guid}/apps/{appId:guid}", async (
            Guid accountId,
            Guid appId,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new RemoveAccountAppCommand(accountId, appId), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Accounts")
        .RequirePermissions(Permissions.App.AssignUsers)
        .WithSummary("Auth_RemoveAccountApp")
        .WithDisplayName("Remove Account from App API")
        .Produces<ApiResponse<object>>();
    }
}
