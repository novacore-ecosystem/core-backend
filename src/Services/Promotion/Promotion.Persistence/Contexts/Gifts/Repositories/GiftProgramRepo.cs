using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Gifts.Repositories;

public sealed class GiftProgramRepo(PromotionDbContext dbContext)
    : PromotionBaseRepository<GiftProgram, Guid>(dbContext), IGiftProgramRepository
{
}
