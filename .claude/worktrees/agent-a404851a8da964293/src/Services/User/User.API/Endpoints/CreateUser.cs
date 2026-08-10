using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.Infrastructure.Idempotency;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;
using NovaCore.BuildingBlock.Web.Swagger.EndpointHeader;

using NovaCore.User.Application.Features.Users.Commands.CreateUser;

namespace NovaCore.User.API.Endpoints;

public sealed record CreateUserRequest(
    string Email,
    string UserName,
    string PhoneNumber,
    string FirstName,
    string LastName,
    string[] Roles,
    string TempPassword,
    string MiddleName = "");

public sealed class CreateUserEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## User Registration",
        "",
        "Creates a new user account with email, username, and personal information.",
        "",
        "### Request Body",
        "- **UserId**: Id of the owning account (required, shared identity with Auth service - creation is idempotent per UserId)",
        "- **Email**: User email address (required, must be unique)",
        "- **UserName**: Unique username (required)",
        "- **PhoneNumber**: User phone number (required)",
        "- **FirstName**: User first name (required)",
        "- **MiddleName**: User middle name (optional)",
        "- **LastName**: User last name (required)",
        "- **Roles**: Roles to grant the new Auth Account (Root may grant Admin or User; Admin may only grant User)",
        "- **TempPassword**: Initial password for the new Auth Account; user should be required to change it on first login",
        "",
        "### Response",
        "Returns created user details with assigned UserId and creation timestamp.",
        "",
        "### Response Fields",
        "- **UserId**: Unique identifier for the created user",
        "- **Email**: Confirmed user email address",
        "- **UserName**: Assigned username",
        "- **PhoneNumber**: User phone number",
        "- **FirstName**: User first name",
        "- **LastName**: User last name",
        "",
        "### Error Responses",
        "- **400**: Invalid request, validation failed, or email/username already exists",
        "- **500**: User creation failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/profiles", Handle)
            .WithTags("User")
            .RequirePermissions(Permissions.Users.Manage)
            .Headers([
                new HeaderDefinition(HeaderKeyConstant.CorrelationId, true),
                new HeaderDefinition(HeaderKeyConstant.IdempotencyKey, true, "Ensures this user is only created once, even if the request is retried")
            ])
            .RequireIdempotency()
            .WithName("CreateUser")
            .WithDisplayName("Create User API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateUserResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateUserRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreateUserCommand(
            request.Email.Trim(),
            request.UserName.Trim(),
            request.PhoneNumber.Trim(),
            request.FirstName.Trim(),
            request.MiddleName.Trim(),
            request.LastName.Trim(),
            request.Roles,
            request.TempPassword.Trim());

        var response = await sender.Send(command, ct);

        return Results.Created($"/users/{response.UserId}",
            ApiResponse<CreateUserResponse>.Ok(response));
    }
}
