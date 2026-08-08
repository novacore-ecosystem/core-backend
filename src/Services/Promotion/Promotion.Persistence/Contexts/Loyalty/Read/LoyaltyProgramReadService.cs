using NovaCore.Promotion.Application.Abstractions.Persistence.Loyalty;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Loyalty.Read;

public sealed class LoyaltyProgramReadService(PromotionDbContext dbContext) : ILoyaltyProgramReadService
{
}
