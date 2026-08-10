using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.User.Application.Features.Users.Queries.GetUserDetail;

namespace NovaCore.User.API.Endpoints;

public sealed class GetUserDetailEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get User Detail",
        "",
        "Retrieves full user information by user ID, including roles.",
        "Roles are read from the shared role cache populated by the Auth service -",
        "if nothing is cached for this user yet, Roles is returned as an empty list.",
        "",
        "### Route Parameters",
        "- **userId**: Unique identifier of the user (required, must be valid GUID)",
        "",
        "### Response Fields",
        "- **Id**: Unique user identifier",
        "- **Email**: User email address",
        "- **UserName**: Unique username",
        "- **PhoneNumber**: User phone number",
        "- **FirstName**: User first name",
        "- **LastName**: User last name",
        "- **Status**: User status (1=Active, 2=Inactive, 3=Suspended)",
        "- **Roles**: Roles cached by the Auth service for this user (may be empty)",
        "",
        "### Error Responses",
        "- **404**: User not found",
        "- **400**: Invalid userId format",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/current/detail", Handle)
            .WithTags("User")
            .RequireAuthorization()
            .WithName("GetUserDetail")
            .WithDisplayName("Get User Detail API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetUserDetailResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetUserDetailQuery();
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetUserDetailResponse>.Ok(response));
    }
}
