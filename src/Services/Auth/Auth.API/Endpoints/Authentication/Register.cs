using NovaCore.Auth.Application.Features.Auth.Commands.Register;

using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Swagger.EndpointHeader;

namespace NovaCore.Auth.API.Endpoints.Authentication;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string MiddleName = "");

public sealed class RegisterEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## User Registration",
        "",
        "Creates a new user account with email and password, scoped to the App identified by the",
        "App key header.",
        "",
        $"### Headers",
        $"- **{HeaderKeyConstant.AppKey}**: Stable App identifier the frontend hardcodes (required, must be an active App). Resolved and validated before the account is created, then carried as the app_id claim on the issued token - not required again on subsequent authenticated requests.",
        "",
        "### Request Body",
        "- **Email**: User email address (required, must be unique)",
        "- **Password**: User password (required)",
        "- **FirstName**: User first name (required)",
        "- **MiddleName**: User middle name (optional)",
        "- **LastName**: User last name (required)",
        "- **PhoneNumber**: User phone number (required)",
        "",
        "### Response",
        "Sets HTTP-only cookies for access and refresh tokens. No tokens in response body.",
        "",
        "### Cookies Set",
        "- **AccessToken**: HTTP-only secure cookie (15 min expiry)",
        "- **RefreshToken**: HTTP-only secure cookie (7 days expiry)",
        "",
        "### Error Responses",
        "- **400**: Invalid request or email already exists",
        "- **500**: Registration failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromHeader(Name = HeaderKeyConstant.AppKey)] string? appCode,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new RegisterCommand(
                request.Email.Trim(),
                request.Password.Trim(),
                request.FirstName.Trim(),
                request.LastName.Trim(),
                request.PhoneNumber.Trim(),
                appCode?.Trim() ?? string.Empty,
                request.MiddleName.Trim());
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok(MessageCode.Created);
        })
        .WithTags("Authentication")
        .AllowAnonymous()
        .Headers([
            new HeaderDefinition(HeaderKeyConstant.CorrelationId, true),
            new HeaderDefinition(HeaderKeyConstant.AppKey, true)
        ])
        .WithSummary("Auth_Register")
        .WithDisplayName("Register API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<object>>();
    }
}
