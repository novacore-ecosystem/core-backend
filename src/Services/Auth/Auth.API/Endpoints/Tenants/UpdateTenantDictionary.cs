using System.Text.Json;

using NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantDictionary;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Tenants;

public sealed class UpdateTenantDictionaryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tenants/{id:guid}/dictionary/{language}", async (
            Guid id,
            string language,
            [FromBody] JsonElement dictionary,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new UpdateTenantDictionaryCommand(id, language, dictionary), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Tenants")
        .RequirePermissions(Permissions.Tenant.Manage)
        .WithSummary("Auth_UpdateTenantDictionary")
        .WithDisplayName("Update Tenant Dictionary API")
        .Produces<ApiResponse<object>>();
    }
}
