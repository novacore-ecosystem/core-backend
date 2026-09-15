using NovaCore.Auth.Application.Features.Registrations.Queries.GetRegistrationDefaults;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Registrations;

public sealed class GetRegistrationDefaultsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/apps/{appId:guid}/registration-defaults", async (
            Guid appId,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var response = await sender.Send(new GetRegistrationDefaultsQuery(appId), ct);
            return ApiResponse<RegistrationDefaultsResponse>.Ok(response);
        })
        .WithTags("Registrations")
        .RequirePermissions(Permissions.RegistrationDefaults.View)
        .WithSummary("Auth_GetRegistrationDefaults")
        .WithDisplayName("Get Registration Defaults API")
        .Produces<ApiResponse<RegistrationDefaultsResponse>>();
    }
}
