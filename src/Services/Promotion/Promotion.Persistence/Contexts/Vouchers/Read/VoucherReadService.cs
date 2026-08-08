using NovaCore.Promotion.Application.Abstractions.Persistence.Vouchers;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Vouchers.Read;

public sealed class VoucherReadService(PromotionDbContext dbContext) : IVoucherReadService
{
}
