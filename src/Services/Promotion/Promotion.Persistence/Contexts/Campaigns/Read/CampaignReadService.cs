using NovaCore.Promotion.Application.Abstractions.Persistence.Campaigns;
using NovaCore.Promotion.Persistence.Contexts.Campaigns.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Campaigns.Read;

public sealed class CampaignReadService(ICampaignRepository campaignRepo) : ICampaignReadService
{
}
