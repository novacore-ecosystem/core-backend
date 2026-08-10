using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.ProductCategories.Commands.DeleteProductCategory;

namespace NovaCore.Product.API.Endpoints.ProductCategory;

public sealed class DeleteProductCategoryEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Delete Product Category",
        "",
        "Deletes a product category. Refuses if the category has child categories or is still",
        "assigned to any product.",
        "",
        "### Route Parameters",
        "- **categoryId**: Unique identifier of the category (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: ProductCategory not found",
        "- **409**: Category has children or is still assigned to products",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/categories/{categoryId}", Handle)
            .WithTags("ProductCategory")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("DeleteProductCategory")
            .WithDisplayName("Delete Product Category API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<DeleteProductCategoryResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid categoryId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new DeleteProductCategoryCommand(categoryId), ct);

        return Results.Ok(ApiResponse<DeleteProductCategoryResponse>.Ok(response));
    }
}
