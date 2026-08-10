using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;
using NovaCore.Order.Application.Features.Orders.DTOs;
using NovaCore.Order.Domain.Entities.Orders.Data;
using NovaCore.Order.Domain.ValueObjects;

namespace NovaCore.Order.Persistence.Mapping.Orders;

/// <summary>
/// Application/Persistence conversion boundary for Order.Create - explicit, not Mapster (see
/// docs/04-coding-rules.md's mapping conventions): this is where Money/Quantity get constructed
/// from the primitives CreateOrderRequest carries, so NovaCore.Order.Domain's CreateOrderData always
/// arrives with fully-built Value Objects.
/// </summary>
public static class OrderMapper
{
    public static CreateOrderData ToCreateOrderData(CreateOrderRequest request)
    {
        return new CreateOrderData(
            request.IdempotencyKey,
            request.CreatedById,
            new CreateOrderOwnerData(
                request.Owner.OwnerId,
                request.Owner.OwnerName,
                request.Owner.OwnerEmail,
                request.Owner.OwnerPhone),
            new CreateOrderShippingData(
                request.ShippingInfo.ShippingMethod,
                request.ShippingInfo.ReceiverName,
                request.ShippingInfo.ReceiverPhone,
                request.ShippingInfo.ShippingAddress,
                request.ShippingInfo.Note),
            [.. request.Items
                .Select((item, index) => new CreateOrderItemData(
                    index + 1,
                    item.ProductId,
                    item.VariationId,
                    item.ProductName,
                    item.VariationName,
                    Money.Create(item.UnitPrice),
                    Quantity.Create(item.Quantity)))]);
    }
}
