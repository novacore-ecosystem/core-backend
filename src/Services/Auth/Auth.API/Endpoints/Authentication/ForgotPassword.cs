using NovaCore.Auth.Application.Features.Auth.Commands.ForgotPassword;

namespace NovaCore.Auth.API.Endpoints.Authentication;

public record ForgotPasswordRequest(string Email);

public sealed class ForgotPasswordEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Forgot Password",
        "",
        "Requests a password-reset token for the given email. Always returns the same success",
        "response whether or not the email belongs to an account, to avoid account enumeration.",
        "",
        "### Request Body",
        "- **Email**: The account's email address (required)",
        "",
        "### Response",
        "Always 200 OK - check email for a reset link if the address is registered.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/forgot-password", async (
            [FromBody] ForgotPasswordRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new ForgotPasswordCommand(request.Email.Trim());
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Authentication")
        .AllowAnonymous()
        .WithSummary("Auth_ForgotPassword")
        .WithDisplayName("Forgot Password API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<object>>();
    }
}
