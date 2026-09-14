using NovaCore.Auth.Application.Features.Apps.Commands.UpsertAppTranslation;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Apps;

public record UpsertAppTranslationRequest(string LanguageCode, string DisplayName, string? Description);

public sealed class UpsertAppTranslationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/apps/{id:guid}/translations", async (
            Guid id,
            [FromBody] UpsertAppTranslationRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new UpsertAppTranslationCommand(
                id,
                request.LanguageCode,
                request.DisplayName,
                request.Description);
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Apps")
        .RequirePermissions(Permissions.App.Manage)
        .WithSummary("Auth_UpsertAppTranslation")
        .WithDisplayName("Upsert App Translation API")
        .Produces<ApiResponse<object>>();
    }
}
