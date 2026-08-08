using NovaCore.Promotion.Application.Abstractions.Persistence.Distributions;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Distributions.Read;

public sealed class DistributionJobReadService(PromotionDbContext dbContext) : IDistributionJobReadService
{
}
