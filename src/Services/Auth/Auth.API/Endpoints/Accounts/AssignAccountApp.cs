using NovaCore.Auth.Application.Features.Accounts.Commands.AssignAccountApp;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Accounts;

public sealed class AssignAccountAppEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/accounts/{accountId:guid}/apps/{appId:guid}", async (
            Guid accountId,
            Guid appId,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new AssignAccountAppCommand(accountId, appId), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Accounts")
        .RequirePermissions(Permissions.App.AssignUsers)
        .WithSummary("Auth_AssignAccountApp")
        .WithDisplayName("Assign Account to App API")
        .Produces<ApiResponse<object>>();
    }
}
