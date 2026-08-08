using NovaCore.Promotion.Application.Abstractions.Persistence.Loyalty;
using NovaCore.Promotion.Persistence.Contexts.Loyalty.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Loyalty.Read;

public sealed class LoyaltyProgramReadService(ILoyaltyProgramRepository loyaltyProgramRepo) : ILoyaltyProgramReadService
{
}
