using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.Products.Commands.ReorderVariations;

namespace NovaCore.Product.API.Endpoints.Product;

public sealed record ReorderVariationsRequest(IReadOnlyList<Guid> OrderedVariationIds);

public sealed class ReorderVariationsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Reorder Variations",
        "",
        "Reassigns DisplayOrder for every variation of a product according to the given order.",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "",
        "### Request Body",
        "- **OrderedVariationIds**: Every existing variation id, each exactly once, in the desired display order (required)",
        "",
        "### Error Responses",
        "- **404**: Product not found",
        "- **400**: OrderedVariationIds does not contain exactly every existing variation id",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/{productId}/variations/reorder", Handle)
            .WithTags("Product")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("ReorderVariations")
            .WithDisplayName("Reorder Variations API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<ReorderVariationsResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromBody] ReorderVariationsRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new ReorderVariationsCommand(productId, request.OrderedVariationIds), ct);

        return Results.Ok(ApiResponse<ReorderVariationsResponse>.Ok(response));
    }
}
