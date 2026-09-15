using NovaCore.Auth.Application.Features.Auth.Commands.ConfirmEmail;

namespace NovaCore.Auth.API.Endpoints.Authentication;

public record ConfirmEmailRequest(Guid AccountId, string Token);

public sealed class ConfirmEmailEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Confirm Email",
        "",
        "Completes the registration email-verification flow using the token sent to the",
        "account's email.",
        "",
        "### Request Body",
        "- **AccountId**: The account the link was issued for (required)",
        "- **Token**: The verification token from the email link (required)",
        "",
        "### Error Responses",
        "- **400**: Token is invalid, expired, or already used",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/confirm-email", async (
            [FromBody] ConfirmEmailRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new ConfirmEmailCommand(request.AccountId, request.Token);
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Authentication")
        .AllowAnonymous()
        .WithSummary("Auth_ConfirmEmail")
        .WithDisplayName("Confirm Email API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<object>>();
    }
}
