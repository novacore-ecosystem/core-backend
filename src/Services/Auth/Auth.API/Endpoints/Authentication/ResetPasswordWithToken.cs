using NovaCore.Auth.Application.Features.Auth.Commands.ResetPasswordWithToken;

namespace NovaCore.Auth.API.Endpoints.Authentication;

public record ResetPasswordWithTokenRequest(string Token, string NewPassword);

public sealed class ResetPasswordWithTokenEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Complete Forgot-Password Reset",
        "",
        "Completes the password reset started by Forgot Password, using the single-use token sent",
        "to the account's email. Revokes every existing session for the account on success.",
        "",
        "### Request Body",
        "- **Token**: The reset token from the email link (required)",
        "- **NewPassword**: The new password (required)",
        "",
        "### Error Responses",
        "- **400**: Token is invalid, expired, or already used; or NewPassword fails validation",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/reset-password/complete", async (
            [FromBody] ResetPasswordWithTokenRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new ResetPasswordWithTokenCommand(request.Token, request.NewPassword);
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Authentication")
        .AllowAnonymous()
        .WithSummary("Auth_ResetPasswordWithToken")
        .WithDisplayName("Complete Forgot-Password Reset API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<object>>();
    }
}
