using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.User.Application.Features.Users.Queries.GetUser;

namespace NovaCore.User.API.Endpoints;

public sealed class GetUserEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get User Details",
        "",
        "Retrieves user information by user ID.",
        "",
        "### Route Parameters",
        "- **userId**: Unique identifier of the user (required, must be valid GUID)",
        "",
        "### Response",
        "Returns detailed user information if found.",
        "",
        "### Response Fields",
        "- **UserId**: Unique user identifier",
        "- **Email**: User email address",
        "- **UserName**: Unique username",
        "- **PhoneNumber**: User phone number",
        "- **FirstName**: User first name",
        "- **LastName**: User last name",
        "",
        "### Error Responses",
        "- **404**: User not found",
        "- **400**: Invalid userId format",
        "- **500**: Retrieval failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/{userId}", Handle)
            .WithTags("User")
            .RequirePermissions(Permissions.Users.View)
            .WithName("GetUser")
            .WithDisplayName("Get User API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetUserResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid userId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetUserQuery(userId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetUserResponse>.Ok(response));
    }
}
