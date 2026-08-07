using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Repository;

using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts;

public abstract class ShippingBaseRepository<TEntity>(ShippingDbContext context)
    : GenericRepository<ShippingDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}

public abstract class ShippingBaseRepository<TEntity, TId>(ShippingDbContext context)
    : EntityGenericRepository<ShippingDbContext, TEntity, TId>(context)
    where TEntity : class, IEntity<TId>
{
}
