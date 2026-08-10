using NovaCore.Auth.Application.Features.Auth.Commands.Logout;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints;

public sealed class Logout : ICarterModule
{
    private readonly string[] API_DESC = [
        "## User Logout",
        "",
        "Logs out the current authenticated user by clearing session tokens.",
        "",
        "### Authentication",
        "Requires valid access token in cookies or Authorization header.",
        "",
        "### Response",
        "Returns success status upon logout.",
        "",
        "### Error Responses",
        "- **401**: Unauthorized (not authenticated)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/logout", async (
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new LogoutCommand();
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Authentication")
        .RequireAuthorization()
        .WithSummary("Auth_Logout")
        .WithDisplayName("Logout API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<object>>();
    }
}
