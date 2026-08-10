using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.ProductCategories.Queries.GetProductCategory;

namespace NovaCore.Product.API.Endpoints.ProductCategory;

public sealed class GetProductCategoryEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Product Category Details",
        "",
        "Retrieves product category information by category ID.",
        "",
        "### Route Parameters",
        "- **categoryId**: Unique identifier of the category (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: ProductCategory not found",
        "- **400**: Invalid categoryId format",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories/{categoryId}", Handle)
            .WithTags("ProductCategory")
            .RequireAuthorization()
            .WithName("GetProductCategory")
            .WithDisplayName("Get Product Category API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetProductCategoryResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid categoryId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetProductCategoryQuery(categoryId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetProductCategoryResponse>.Ok(response));
    }
}
