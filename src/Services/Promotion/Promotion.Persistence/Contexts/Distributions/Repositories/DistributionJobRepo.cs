using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Distributions.Repositories;

public sealed class DistributionJobRepo(PromotionDbContext dbContext)
    : PromotionBaseRepository<DistributionJob, Guid>(dbContext), IDistributionJobRepository
{
}
