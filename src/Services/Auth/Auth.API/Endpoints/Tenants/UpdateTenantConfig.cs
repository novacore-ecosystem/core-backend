using System.Text.Json;

using NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantConfig;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

public sealed class UpdateTenantConfigEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tenants/{id:guid}/config", async (
            Guid id,
            [FromQuery] string? language,
            [FromBody] JsonElement config,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var languageCode = language.IsNotNullOrWhiteSpace()
                ? LanguageCode.Create(language!)
                : null;
            var command = new UpdateTenantConfigCommand(id, languageCode, config);
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.Manage)
        .WithSummary("Auth_UpdateTenantConfig")
        .WithDisplayName("Update Tenant Config API")
        .Produces<ApiResponse<object>>();
    }
}
