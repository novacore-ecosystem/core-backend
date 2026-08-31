using NovaCore.Auth.Application.Features.Tenants.Commands.UpsertTenantTranslation;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

public record UpsertTenantTranslationRequest(string Language, string Key, string Value);

public sealed class UpsertTenantTranslationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tenants/{id:guid}/translations", async (
            Guid id,
            [FromBody] UpsertTenantTranslationRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(
                new UpsertTenantTranslationCommand(id, request.Language, request.Key, request.Value),
                ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.Manage)
        .WithSummary("Auth_UpsertTenantTranslation")
        .WithDisplayName("Upsert Tenant Translation API")
        .Produces<ApiResponse<object>>();
    }
}
