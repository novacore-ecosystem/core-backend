using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Recommendations.Repositories;

public sealed class RecommendationProgramRepo(PromotionDbContext dbContext)
    : PromotionBaseRepository<RecommendationProgram, Guid>(dbContext), IRecommendationProgramRepository
{
}
