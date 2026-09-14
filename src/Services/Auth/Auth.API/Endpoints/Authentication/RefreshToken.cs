using NovaCore.Auth.Application.Features.Auth.Commands.RefreshToken;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints.Authentication;

public sealed class RefreshTokenEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Refresh Access Token",
        "",
        "Generates a new access token using the refresh token from cookies, re-validated against",
        "the App identified by the App key header.",
        "",
        $"### Headers",
        $"- **{HeaderKeyConstant.AppKey}**: Stable App identifier the frontend hardcodes (required, must be an active App). The refreshed token carries this as its app_id claim.",
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
            [FromHeader(Name = HeaderKeyConstant.AppKey)] string? appCode,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new RefreshTokenCommand(appCode?.Trim() ?? string.Empty);
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
