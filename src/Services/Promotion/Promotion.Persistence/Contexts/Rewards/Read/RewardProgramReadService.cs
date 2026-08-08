using NovaCore.Promotion.Application.Abstractions.Persistence.Rewards;
using NovaCore.Promotion.Persistence.Contexts.Rewards.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Rewards.Read;

public sealed class RewardProgramReadService(IRewardProgramRepository rewardProgramRepo) : IRewardProgramReadService
{
}
