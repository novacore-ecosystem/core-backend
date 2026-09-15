using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Features.Auth.Commands.ResendEmail;

namespace NovaCore.Auth.API.Endpoints.Authentication;

public record ResendEmailRequest(string Email, string Purpose);

public sealed class ResendEmailEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Resend Authentication Email",
        "",
        "Resends an EmailVerification or PasswordReset email. Always returns the same success",
        "response regardless of whether the email belongs to an account, to avoid account",
        "enumeration - identical to Forgot Password's existing behavior.",
        "",
        "### Request Body",
        "- **Email**: The account's email address (required)",
        "- **Purpose**: `EmailVerification` or `PasswordReset` (required)",
        "",
        "### Error Responses",
        "- **400**: Invalid request, or a resend was already requested for this email/purpose",
        "  within the last 30 seconds - `Details.remainingSeconds` carries the wait time.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/resend-email", async (
            [FromBody] ResendEmailRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            Enum.TryParse<AuthMailPurpose>(request.Purpose, ignoreCase: true, out var purpose);

            var command = new ResendEmailCommand(request.Email.Trim(), purpose);
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Authentication")
        .AllowAnonymous()
        .WithSummary("Auth_ResendEmail")
        .WithDisplayName("Resend Authentication Email API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<object>>();
    }
}
