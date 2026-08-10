using NovaCore.Auth.Application.Features.Auth.Commands.Login;

namespace NovaCore.Auth.API.Endpoints;

public record LoginRequest(string Email, string Password);

public sealed class Login : ICarterModule
{
    private readonly string[] API_DESC = [
        "## User Login",
        "",
        "Authenticates a user with email and password credentials.",
        "",
        "### Request Body",
        "- **Email**: User email address (required)",
        "- **Password**: User password (required)",
        "",
        "### Response",
        "Sets HTTP-only cookies for access and refresh tokens. No tokens in response body.",
        "",
        "### Cookies Set",
        "- **AccessToken**: HTTP-only secure cookie (15 min expiry)",
        "- **RefreshToken**: HTTP-only secure cookie (7 days expiry)",
        "",
        "### Error Responses",
        "- **401**: Invalid credentials",
        "- **400**: Invalid request parameters",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new LoginCommand(
                request.Email.Trim(),
                request.Password.Trim());
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Authentication")
        .AllowAnonymous()
        .WithSummary("Auth_Login")
        .WithDisplayName("Login API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<object>>();
    }
}
