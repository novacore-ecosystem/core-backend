using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Promotion.Persistence.Contexts.Promotions.Repositories;

public interface IPromotionRepository : IRepository<PromotionEntity, Guid>
{
}
