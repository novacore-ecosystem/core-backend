using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence.Ef.UnitOfWork;

namespace NovaCore.Shipping.Persistence.Engine.UnitOfWork;

public sealed class UnitOfWork(ShippingDbContext context)
    : EfUnitOfWork<ShippingDbContext>(context), IUnitOfWork
{
}
