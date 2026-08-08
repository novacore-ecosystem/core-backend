using NovaCore.Promotion.Application.Abstractions.Persistence.Recommendations;
using NovaCore.Promotion.Persistence.Contexts.Recommendations.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Recommendations.Read;

public sealed class RecommendationProgramReadService(IRecommendationProgramRepository recommendationProgramRepo)
    : IRecommendationProgramReadService
{
}
