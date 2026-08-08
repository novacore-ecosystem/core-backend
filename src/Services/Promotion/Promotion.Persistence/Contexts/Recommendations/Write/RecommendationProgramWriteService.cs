using NovaCore.Promotion.Application.Abstractions.Persistence.Recommendations;
using NovaCore.Promotion.Persistence.Contexts.Recommendations.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Recommendations.Write;

public sealed class RecommendationProgramWriteService(IRecommendationProgramRepository recommendationProgramRepo)
    : IRecommendationProgramWriteService
{
}
