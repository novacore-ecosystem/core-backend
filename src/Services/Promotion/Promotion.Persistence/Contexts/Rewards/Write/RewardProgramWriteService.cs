using NovaCore.Promotion.Application.Abstractions.Persistence.Rewards;
using NovaCore.Promotion.Persistence.Contexts.Rewards.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Rewards.Write;

public sealed class RewardProgramWriteService(IRewardProgramRepository rewardProgramRepo) : IRewardProgramWriteService
{
}
