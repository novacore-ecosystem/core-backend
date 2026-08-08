using NovaCore.Promotion.Application.Abstractions.Persistence.Recommendations;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Recommendations.Read;

public sealed class RecommendationProgramReadService(PromotionDbContext dbContext) : IRecommendationProgramReadService
{
}
