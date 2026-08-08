using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Loyalty.Repositories;

public sealed class LoyaltyProgramRepo(PromotionDbContext dbContext)
    : PromotionBaseRepository<LoyaltyProgram, Guid>(dbContext), ILoyaltyProgramRepository
{
}
