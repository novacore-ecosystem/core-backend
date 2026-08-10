using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Product.Application.Features.Products.DTOs;

namespace NovaCore.Product.Application.Features.Products.Mapping;

public static class VariantMapping
{
    public static ProductVariant MapInputToEntity(
        this VariantInputDto dto,
        Guid productId,
        int displayOrder)
    {
        return ProductVariant.Create(
            productId: productId,
            sku: Sku.Create(dto.Sku),
            name: dto.Name,
            price: Money.Create(dto.Price),
            displayOrder: displayOrder,
            barcode: dto.Barcode is not null ? Barcode.Create(dto.Barcode) : null,
            weight: MapWeight(dto.Weight, dto.WeightUnit),
            dimensions: dto.DimensionsLength.HasValue
                && dto.DimensionsWidth.HasValue
                && dto.DimensionsHeight.HasValue
                    ? Dimensions.Create(
                        dto.DimensionsLength.Value,
                        dto.DimensionsWidth.Value,
                        dto.DimensionsHeight.Value)
                    : null,
            images: dto.Images,
            status: VariantStatus.Active,
            metadata: null);
    }

    public static IEnumerable<ProductVariant> MapInputToEntities(
        this IEnumerable<VariantInputDto> dtos,
        Guid productId,
        int beginDisplayOrder = 1)
    {
        foreach (var dto in dtos)
        {
            var entity = dto.MapInputToEntity(productId, beginDisplayOrder);
            if (dto.IsDefault)
                entity.MarkAsDefault();

            beginDisplayOrder++;
            yield return entity;
        }
    }

    public static Weight? MapWeight(decimal? weight, string? unit)
    {
        if (!weight.HasValue && unit is null) return null;
        if (!Enum.TryParse<WeightUnit>(unit, out var weightUnit))
            throw new BadRequestException("Weight Unit is invalid.");

        return MapWeight(weight, weightUnit);
    }

    public static Weight? MapWeight(decimal? weight, WeightUnit? unit)
    {
        if ((weight.HasValue && !unit.HasValue) || (!weight.HasValue && unit.HasValue))
            throw new BadRequestException("Weight information is invalid");

        if (!weight.HasValue && !unit.HasValue)
            return null;

        return Weight.Create(weight!.Value, unit!.Value);
    }
}
