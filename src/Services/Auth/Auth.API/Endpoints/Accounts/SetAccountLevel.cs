using NovaCore.Auth.Application.Features.Accounts.Commands.SetAccountLevel;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints.Accounts;

public record SetAccountLevelRequest(int Level);

public sealed class SetAccountLevelEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/accounts/{id:guid}/level", async (
            Guid id,
            [FromBody] SetAccountLevelRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new SetAccountLevelCommand(id, request.Level), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Accounts")
        .RequirePermissions(Permissions.Account.Manage)
        .WithSummary("Auth_SetAccountLevel")
        .WithDisplayName("Set Account Level API")
        .Produces<ApiResponse<object>>();
    }
}
