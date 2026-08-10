using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Repository;

using NovaCore.Payment.Persistence.Engine;

namespace NovaCore.Payment.Persistence.Contexts;

public abstract class PaymentBaseRepository<TEntity>(PaymentDbContext context)
    : GenericRepository<PaymentDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}

public abstract class PaymentBaseRepository<TEntity, TId>(PaymentDbContext context)
    : EntityGenericRepository<PaymentDbContext, TEntity, TId>(context)
    where TEntity : class, IEntity<TId>
{
}
