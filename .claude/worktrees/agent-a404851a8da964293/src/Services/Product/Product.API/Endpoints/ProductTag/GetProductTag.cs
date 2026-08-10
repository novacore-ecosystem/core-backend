using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.ProductTags.Queries.GetProductTag;

namespace NovaCore.Product.API.Endpoints.ProductTag;

public sealed class GetProductTagEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Product Tag Details",
        "",
        "Retrieves product tag information by tag ID.",
        "",
        "### Route Parameters",
        "- **tagId**: Unique identifier of the tag (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: ProductTag not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tags/{tagId}", Handle)
            .WithTags("ProductTag")
            .RequireAuthorization()
            .WithName("GetProductTag")
            .WithDisplayName("Get Product Tag API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetProductTagResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid tagId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetProductTagQuery(tagId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetProductTagResponse>.Ok(response));
    }
}
