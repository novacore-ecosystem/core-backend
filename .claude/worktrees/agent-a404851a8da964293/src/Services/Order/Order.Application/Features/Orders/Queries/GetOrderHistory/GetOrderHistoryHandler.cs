using NovaCore.BuildingBlock.Application.Abstractions.Common;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;

namespace NovaCore.Order.Application.Features.Orders.Queries.GetOrderHistory;

public sealed class GetOrderHistoryHandler(
    ICurrentUserService currentUser,
    IOrderReadService orderReadService) : IQueryHandler<GetOrderHistoryQuery, PaginatedResult<OrderHistoryItemResponse>>
{
    public async Task<PaginatedResult<OrderHistoryItemResponse>> Handle(
        GetOrderHistoryQuery request,
        CancellationToken ct = default)
    {
        var result = await orderReadService.SearchByCustomerAsync(
            currentUser.GetUserId() ?? throw new ForbiddenException(),
            request.Criteria,
            ct);
        var items = result.Items
            .Select(o => new OrderHistoryItemResponse(
                o.Id,
                o.Status,
                o.GrandTotal.Value,
                o.CreatedAt,
                o.UpdatedAt))
            .ToList();

        return PaginatedResult<OrderHistoryItemResponse>.Create(
            items,
            result.PageNumber,
            result.PageSize,
            result.TotalCount);
    }
}
