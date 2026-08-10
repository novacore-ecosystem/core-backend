using NovaCore.Order.Persistence.Engine;

namespace NovaCore.Order.Persistence.Contexts.Orders.Repositories;

public sealed class OrderRepo(OrderDbContext dbContext)
    : OrderBaseRepository<OrderEntity, Guid>(dbContext), IOrderRepository
{
    public async Task<OrderEntity?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken ct = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Owner)
            .Include(o => o.Price)
            .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey, ct);
    }
}
