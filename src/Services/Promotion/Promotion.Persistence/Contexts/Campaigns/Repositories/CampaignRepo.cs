using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Campaigns.Repositories;

public sealed class CampaignRepo(PromotionDbContext dbContext)
    : PromotionBaseRepository<Campaign, Guid>(dbContext), ICampaignRepository
{
}
