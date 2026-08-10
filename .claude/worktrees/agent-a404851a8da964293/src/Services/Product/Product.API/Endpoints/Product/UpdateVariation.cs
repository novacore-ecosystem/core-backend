using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.Products.Commands.UpdateVariation;
using NovaCore.Product.Domain.Enums;

namespace NovaCore.Product.API.Endpoints.Product;

public sealed record UpdateVariationRequest(
    string Sku,
    string Name,
    decimal Price,
    string Status,
    string? Barcode = null,
    decimal? Weight = null,
    string? WeightUnit = null,
    decimal? DimensionsLength = null,
    decimal? DimensionsWidth = null,
    decimal? DimensionsHeight = null,
    IReadOnlyCollection<string>? Images = null);

public sealed class UpdateVariationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update Variation",
        "",
        "Updates a Variant's details (Sku, Name, Barcode, Price, Cost, Weight, Dimensions,",
        "Images, Status). Never touches DisplayOrder or IsDefault - use ReorderVariations /",
        "ChangeDefaultVariation for those.",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "- **variationId**: Unique identifier of the variation (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Product or variation not found",
        "- **409**: Sku already exists",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{productId}/variations/{variationId}", Handle)
            .WithTags("Product")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("UpdateVariation")
            .WithDisplayName("Update Variation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<UpdateVariationResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromRoute] Guid variationId,
        [FromBody] UpdateVariationRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<VariantStatus>(request.Status.Trim(), out var variationStatus))
            throw new BadRequestException($"Variation Status ({request.Status}) is invalid.");

        if (request.WeightUnit != null && !Enum.TryParse<WeightUnit>(request.WeightUnit.Trim(), out var _))
            throw new BadRequestException($"Weight Unit ({request.WeightUnit}) is invalid.");

        var command = new UpdateVariationCommand(
            productId,
            variationId,
            request.Sku.Trim(),
            request.Name.Trim(),
            request.Price,
            variationStatus,
            request.Barcode?.Trim(),
            request.Weight,
            request.WeightUnit != null
                ? Enum.Parse<WeightUnit>(request.WeightUnit.Trim())
                : null,
            request.DimensionsLength,
            request.DimensionsWidth,
            request.DimensionsHeight,
            request.Images);

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<UpdateVariationResponse>.Ok(response));
    }
}
