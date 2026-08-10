using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.Order.Application.Features.Orders.Queries.GetOrderHistory;

public sealed record GetOrderHistoryQuery(CriteriaRequest Criteria) : IQuery<PaginatedResult<OrderHistoryItemResponse>>;

public sealed record OrderHistoryItemResponse(
    Guid Id,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
