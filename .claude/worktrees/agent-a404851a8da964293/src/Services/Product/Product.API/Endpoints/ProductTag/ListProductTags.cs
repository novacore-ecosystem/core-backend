using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.ProductTags.Queries.ListProductTags;

namespace NovaCore.Product.API.Endpoints.ProductTag;

public sealed class ListProductTagsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## List Product Tags",
        "",
        "Returns the full flat tag set. Not paginated - tag counts are small reference data.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tags", Handle)
            .WithTags("ProductTag")
            .RequireAuthorization()
            .WithName("ListProductTags")
            .WithDisplayName("List Product Tags API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<ListProductTagsResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new ListProductTagsQuery(), ct);

        return Results.Ok(ApiResponse<ListProductTagsResponse>.Ok(response));
    }
}
