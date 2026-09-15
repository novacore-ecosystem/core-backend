using NovaCore.Auth.Application.Features.Auth.Commands.ResetPassword;

namespace NovaCore.Auth.API.Endpoints.Authentication;

public record ResetPasswordRequest(string CurrentPassword, string NewPassword);

public sealed class ResetPasswordEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Reset Password",
        "",
        "Changes the authenticated account's password. Requires the current password and revokes",
        "every other active session for the account once the change succeeds.",
        "",
        "### Request Body",
        "- **CurrentPassword**: The account's current password (required)",
        "- **NewPassword**: The new password (required)",
        "",
        "### Response",
        "Clears the caller's own access/refresh token cookies - re-authentication is required.",
        "",
        "### Error Responses",
        "- **401**: Not authenticated, or CurrentPassword is incorrect",
        "- **400**: NewPassword fails validation",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/reset-password", async (
            [FromBody] ResetPasswordRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new ResetPasswordCommand(request.CurrentPassword, request.NewPassword);
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Authentication")
        .WithSummary("Auth_ResetPassword")
        .WithDisplayName("Reset Password API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<object>>();
    }
}
