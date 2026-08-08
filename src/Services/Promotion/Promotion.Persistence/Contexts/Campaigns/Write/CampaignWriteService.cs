using NovaCore.Promotion.Application.Abstractions.Persistence.Campaigns;
using NovaCore.Promotion.Persistence.Contexts.Campaigns.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Campaigns.Write;

public sealed class CampaignWriteService(ICampaignRepository campaignRepo) : ICampaignWriteService
{
}
