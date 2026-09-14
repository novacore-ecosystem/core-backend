using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence.Ef.UnitOfWork;

namespace NovaCore.Shipping.Persistence.Engine.UnitOfWork;

public sealed class UnitOfWork(
    ShippingDbContext context,
    Func<IReadOnlyList<Guid>, CancellationToken, Task>? notifyOutboxCommittedAsync = null)
    : EfUnitOfWork<ShippingDbContext>(context, notifyOutboxCommittedAsync), IUnitOfWork
{
}
