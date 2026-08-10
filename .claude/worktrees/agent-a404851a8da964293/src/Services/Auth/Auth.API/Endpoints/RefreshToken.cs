using NovaCore.Auth.Application.Features.Auth.Commands.RefreshToken;

namespace NovaCore.Auth.API.Endpoints;

public sealed class RefreshToken : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Refresh Access Token",
        "",
        "Generates a new access token using the refresh token from cookies.",
        "",
        "### Request",
        "No request body required. Refresh token sent automatically via HTTP-only cookie.",
        "",
        "### Response",
        "Sets new HTTP-only cookies for access and refresh tokens. No tokens in response body.",
        "",
        "### Cookies Required",
        "- **RefreshToken**: HTTP-only cookie with refresh token (required)",
        "",
        "### Cookies Set",
        "- **AccessToken**: New HTTP-only secure cookie (15 min expiry)",
        "- **RefreshToken**: New HTTP-only secure cookie (7 days expiry)",
        "",
        "### Error Responses",
        "- **400**: Refresh token missing from cookies",
        "- **401**: Refresh token is invalid or expired",
        "- **404**: User not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/refresh-token", async (
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new RefreshTokenCommand();
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Authentication")
        .AllowAnonymous()
        .WithSummary("Auth_RefreshToken")
        .WithDisplayName("Refresh Token API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<object>>();
    }
}
