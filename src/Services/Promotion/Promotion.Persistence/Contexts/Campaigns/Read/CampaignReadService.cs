using NovaCore.Promotion.Application.Abstractions.Persistence.Campaigns;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Campaigns.Read;

public sealed class CampaignReadService(PromotionDbContext dbContext) : ICampaignReadService
{
}
