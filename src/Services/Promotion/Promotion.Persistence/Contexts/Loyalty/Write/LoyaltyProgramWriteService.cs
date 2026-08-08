using NovaCore.Promotion.Application.Abstractions.Persistence.Loyalty;
using NovaCore.Promotion.Persistence.Contexts.Loyalty.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Loyalty.Write;

public sealed class LoyaltyProgramWriteService(ILoyaltyProgramRepository loyaltyProgramRepo) : ILoyaltyProgramWriteService
{
}
