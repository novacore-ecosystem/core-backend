using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Payment.Persistence.Contexts.Refunds.Repositories;

public interface IRefundRepository : IRepository<Refund, Guid>
{
}
