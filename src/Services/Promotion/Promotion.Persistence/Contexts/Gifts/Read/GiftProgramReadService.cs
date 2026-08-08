using NovaCore.Promotion.Application.Abstractions.Persistence.Gifts;
using NovaCore.Promotion.Persistence.Contexts.Gifts.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Gifts.Read;

public sealed class GiftProgramReadService(IGiftProgramRepository giftProgramRepo) : IGiftProgramReadService
{
}
