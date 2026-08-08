using NovaCore.Promotion.Application.Abstractions.Persistence.Distributions;
using NovaCore.Promotion.Persistence.Contexts.Distributions.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Distributions.Write;

public sealed class DistributionJobWriteService(IDistributionJobRepository distributionJobRepo) : IDistributionJobWriteService
{
}
