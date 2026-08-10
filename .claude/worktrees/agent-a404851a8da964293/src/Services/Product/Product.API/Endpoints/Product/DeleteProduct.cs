using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.Products.Commands.DeleteProduct;

namespace NovaCore.Product.API.Endpoints.Product;

public sealed class DeleteProductEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Delete Product",
        "",
        "Permanently deletes a product together with all of its variations (hard delete).",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Product not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{productId}", Handle)
            .WithTags("Product")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("DeleteProduct")
            .WithDisplayName("Delete Product API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<DeleteProductResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new DeleteProductCommand(productId), ct);

        return Results.Ok(ApiResponse<DeleteProductResponse>.Ok(response));
    }
}
