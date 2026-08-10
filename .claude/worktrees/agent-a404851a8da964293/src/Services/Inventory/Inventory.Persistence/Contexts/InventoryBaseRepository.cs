using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Repository;

using NovaCore.Inventory.Persistence.Engine;

namespace NovaCore.Inventory.Persistence.Contexts;

public abstract class InventoryBaseRepository<TEntity>(InventoryDbContext context)
    : GenericRepository<InventoryDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}

public abstract class InventoryBaseRepository<TEntity, TId>(InventoryDbContext context)
    : EntityGenericRepository<InventoryDbContext, TEntity, TId>(context)
    where TEntity : class, IEntity<TId>
{
}
