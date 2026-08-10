using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Repository;

using NovaCore.Product.Persistence.Engine;

namespace NovaCore.Product.Persistence.Contexts;

public abstract class ProductBaseRepository<TEntity>(ProductDbContext context)
    : GenericRepository<ProductDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}
