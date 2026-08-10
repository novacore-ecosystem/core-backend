using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.Order.Application.Abstractions.Persistence.Orders;

public interface IOrderReadService
{
    Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<OrderEntity>> SearchAsync(CriteriaRequest request, CancellationToken ct = default);

    /// <summary>Same as <see cref="SearchAsync"/>, scoped to one customer's own orders - the scope is applied here, at the query, so it can't be bypassed via the request's filters.</summary>
    Task<PaginatedResult<OrderEntity>> SearchByCustomerAsync(Guid ownerId, CriteriaRequest request, CancellationToken ct = default);

    /// <summary>Backs CreateOrderHandler's idempotency check - a prior order with the same (CustomerId, IdempotencyKey) is returned as-is instead of creating a duplicate.</summary>
    Task<OrderEntity?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);
}
