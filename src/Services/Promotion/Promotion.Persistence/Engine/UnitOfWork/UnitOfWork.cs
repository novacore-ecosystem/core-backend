using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence.Ef.UnitOfWork;

namespace NovaCore.Promotion.Persistence.Engine.UnitOfWork;

public sealed class UnitOfWork(
    PromotionDbContext context,
    Func<IReadOnlyList<Guid>, CancellationToken, Task>? notifyOutboxCommittedAsync = null)
    : EfUnitOfWork<PromotionDbContext>(context, notifyOutboxCommittedAsync), IUnitOfWork
{
}
