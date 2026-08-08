using NovaCore.Promotion.Application.Abstractions.Persistence.Gifts;
using NovaCore.Promotion.Persistence.Contexts.Gifts.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Gifts.Write;

public sealed class GiftProgramWriteService(IGiftProgramRepository giftProgramRepo) : IGiftProgramWriteService
{
}
