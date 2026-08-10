using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.Products.Commands.UpdateProduct;

namespace NovaCore.Product.API.Endpoints.Product;

public sealed record UpdateProductRequest(string Name, string Description, string Slug);

public sealed class UpdateProductEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update Product",
        "",
        "Updates Product-level shared information only (Name/Description/Slug). Never touches",
        "Variant data - use the dedicated Variant APIs for that.",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "",
        "### Request Body",
        "- **Name**: Product name (required)",
        "- **Description**: Product description",
        "- **Slug**: Unique URL slug (required, must be unique)",
        "",
        "### Error Responses",
        "- **404**: Product not found",
        "- **400**: Invalid productId format or validation failed",
        "- **409**: Slug already exists",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{productId}", Handle)
            .WithTags("Product")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("UpdateProduct")
            .WithDisplayName("Update Product API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<UpdateProductResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromBody] UpdateProductRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new UpdateProductCommand(
            productId,
            request.Name.Trim(),
            request.Description?.Trim() ?? string.Empty,
            request.Slug.Trim());

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<UpdateProductResponse>.Ok(response));
    }
}
