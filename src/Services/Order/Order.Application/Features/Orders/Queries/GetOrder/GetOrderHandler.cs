using NovaCore.BuildingBlock.Application.Abstractions.Authorization;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;

namespace NovaCore.Order.Application.Features.Orders.Queries.GetOrder;

public sealed class GetOrderHandler(
    IAuthorizationGuard authorizationGuard,
    ICurrentUserService currentUser,
    IOrderReadService orderReadService) : IQueryHandler<GetOrderQuery, GetOrderResponse>
{
    public async Task<GetOrderResponse> Handle(GetOrderQuery request, CancellationToken ct = default)
    {
        var order = await orderReadService.GetByIdAsync(request.OrderId, ct)
            ?? throw new NotFoundException("Order", request.OrderId);

        // Endpoint only requires RequireAuthenticated (any logged-in user), not a permission, so
        // the owner-vs-admin distinction has to happen here: an admin dashboard needs to view any
        // order, but a regular customer may only view their own.
        var canViewAnyOrder = authorizationGuard.HasPermissions(Permissions.Order.View);
        if (!canViewAnyOrder && order.Owner.OwnerId != currentUser.GetUserId())
            throw new ForbiddenException();

        return new GetOrderResponse(
            order.Id,
            order.Owner.OwnerId,
            order.Owner.OwnerName,
            order.Owner.OwnerPhone.Value,
            order.Shipping.Address,
            order.Status,
            order.GrandTotal.Value,
            MapToItemResponse(order.Items),
            order.CancellationReason,
            order.CreatedAt,
            order.UpdatedAt);
    }

    private static GetOrderItemResponse[] MapToItemResponse(IEnumerable<OrderItem> items)
    {
        return [.. items
            .Select(i => new GetOrderItemResponse(
                i.ProductId,
                i.ProductName,
                i.UnitPrice.Value,
                i.Quantity.Value,
                0,
                i.FinalAmount.Value))];
    }
}
