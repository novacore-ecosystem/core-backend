using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.Products.Commands.RemoveProductTag;

namespace NovaCore.Product.API.Endpoints.Product;

public sealed class RemoveProductTagEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Remove Product Tag",
        "",
        "Removes a tag assignment from a product (idempotent).",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "- **tagId**: Tag to remove (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Product not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{productId}/tags/{tagId}", Handle)
            .WithTags("Product")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("RemoveProductTag")
            .WithDisplayName("Remove Product Tag API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<RemoveProductTagResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromRoute] Guid tagId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new RemoveProductTagCommand(productId, tagId), ct);

        return Results.Ok(ApiResponse<RemoveProductTagResponse>.Ok(response));
    }
}
