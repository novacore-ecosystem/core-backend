using NovaCore.Promotion.Application.Abstractions.Persistence.Distributions;
using NovaCore.Promotion.Persistence.Contexts.Distributions.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Distributions.Read;

public sealed class DistributionJobReadService(IDistributionJobRepository distributionJobRepo) : IDistributionJobReadService
{
}
