using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.Products.Commands.AssignProductCategory;

namespace NovaCore.Product.API.Endpoints.Product;

public sealed class AssignProductCategoryEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Assign Product Category",
        "",
        "Assigns a category to a product (many-to-many; idempotent).",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "- **categoryId**: Category to assign (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Product or category not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/{productId}/categories/{categoryId}", Handle)
            .WithTags("Product")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("AssignProductCategory")
            .WithDisplayName("Assign Product Category API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<AssignProductCategoryResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromRoute] Guid categoryId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new AssignProductCategoryCommand(productId, categoryId), ct);

        return Results.Ok(ApiResponse<AssignProductCategoryResponse>.Ok(response));
    }
}
