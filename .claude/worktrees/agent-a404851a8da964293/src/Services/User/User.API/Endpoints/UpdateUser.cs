using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.User.Application.Features.Users.Commands.UpdateUser;

namespace NovaCore.User.API.Endpoints;

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string MiddleName = "");

public sealed class UpdateUserEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update User Information",
        "",
        "Updates user profile information including name and phone number.",
        "",
        "### Route Parameters",
        "- **userId**: Unique identifier of the user to update (required, must be valid GUID)",
        "",
        "### Request Body",
        "- **FirstName**: User first name (required)",
        "- **MiddleName**: User middle name (optional)",
        "- **LastName**: User last name (required)",
        "- **PhoneNumber**: User phone number (required)",
        "",
        "### Response",
        "Returns updated user information.",
        "",
        "### Response Fields",
        "- **UserId**: Unique user identifier",
        "- **Email**: User email address",
        "- **UserName**: Unique username",
        "- **PhoneNumber**: Updated phone number",
        "- **FirstName**: Updated first name",
        "- **LastName**: Updated last name",
        "",
        "### Error Responses",
        "- **404**: User not found",
        "- **400**: Invalid userId format or validation failed",
        "- **500**: Update failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/profiles/{userId}", Handle)
            .WithTags("User")
            .RequirePermissions(Permissions.Users.Manage)
            .WithName("UpdateUser")
            .WithDisplayName("Update User API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<UpdateUserResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new UpdateUserCommand(
            userId,
            request.FirstName.Trim(),
            request.MiddleName.Trim(),
            request.LastName.Trim(),
            request.PhoneNumber.Trim());

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<UpdateUserResponse>.Ok(response));
    }
}
