using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Ef.Criteria;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;
using NovaCore.Order.Application.Features.Orders.Search;
using NovaCore.Order.Persistence.Contexts.Orders.Repositories;
using NovaCore.Order.Persistence.Engine;

namespace NovaCore.Order.Persistence.Contexts.Orders.Read;

public sealed class OrderReadService(
    IOrderRepository orderRepo,
    OrderDbContext dbContext) : IOrderReadService
{
    public async Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await orderRepo.GetByIdAsync(
            id,
            query => query
                .Include(q => q.Items)
                .Include(q => q.Owner)
                .Include(q => q.Price)
                .Include(q => q.Cancellation),
            ct);
    }

    public async Task<PaginatedResult<OrderEntity>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Owner)
            .Include(o => o.Price)
            .Include(o => o.Cancellation)
            .ApplyCriteria(OrderCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }

    public async Task<PaginatedResult<OrderEntity>> SearchByCustomerAsync(Guid customerId, CriteriaRequest request, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Owner)
            .Include(o => o.Price)
            .Include(o => o.Cancellation)
            .Where(o => o.Owner.OwnerId == customerId)
            .ApplyCriteria(OrderHistoryCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }

    public async Task<OrderEntity?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken ct = default)
        => await orderRepo.GetByIdempotencyKeyAsync(idempotencyKey, ct);
}
