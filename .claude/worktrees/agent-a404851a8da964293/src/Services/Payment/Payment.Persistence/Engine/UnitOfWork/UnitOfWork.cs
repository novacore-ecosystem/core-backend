using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence.Ef.UnitOfWork;

namespace NovaCore.Payment.Persistence.Engine.UnitOfWork;

public sealed class UnitOfWork(PaymentDbContext context)
    : EfUnitOfWork<PaymentDbContext>(context), IUnitOfWork
{
}
