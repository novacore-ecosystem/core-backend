using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.Products.Commands.RemoveProductCategory;

namespace NovaCore.Product.API.Endpoints.Product;

public sealed class RemoveProductCategoryEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Remove Product Category",
        "",
        "Removes a category assignment from a product (idempotent).",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "- **categoryId**: Category to remove (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Product not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{productId}/categories/{categoryId}", Handle)
            .WithTags("Product")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("RemoveProductCategory")
            .WithDisplayName("Remove Product Category API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<RemoveProductCategoryResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromRoute] Guid categoryId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new RemoveProductCategoryCommand(productId, categoryId), ct);

        return Results.Ok(ApiResponse<RemoveProductCategoryResponse>.Ok(response));
    }
}
