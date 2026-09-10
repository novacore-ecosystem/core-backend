using NovaCore.Auth.Application.Features.Accounts.Queries.GetAccountAuthorization;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints.Accounts;

public sealed class GetAccountAuthorizationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/accounts/{id:guid}/authorization", async (
            Guid id,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var response = await sender.Send(new GetAccountAuthorizationQuery(id), ct);
            return ApiResponse<AccountAuthorizationResponse>.Ok(response);
        })
        .WithTags("Accounts")
        .RequirePermissions(Permissions.Account.View)
        .WithSummary("Auth_GetAccountAuthorization")
        .WithDisplayName("Get Account Authorization API")
        .Produces<ApiResponse<AccountAuthorizationResponse>>();
    }
}
