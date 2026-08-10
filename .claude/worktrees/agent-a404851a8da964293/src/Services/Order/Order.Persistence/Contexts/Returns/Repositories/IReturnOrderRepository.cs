using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Order.Persistence.Contexts.Returns.Repositories;

public interface IReturnOrderRepository : IRepository<ReturnOrder, Guid>
{
}
