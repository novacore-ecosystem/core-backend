using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Rewards.Repositories;

public sealed class RewardProgramRepo(PromotionDbContext dbContext)
    : PromotionBaseRepository<RewardProgram, Guid>(dbContext), IRewardProgramRepository
{
}
