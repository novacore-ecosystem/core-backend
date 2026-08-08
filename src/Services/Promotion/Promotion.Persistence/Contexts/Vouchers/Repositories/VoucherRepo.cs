using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Vouchers.Repositories;

public sealed class VoucherRepo(PromotionDbContext dbContext)
    : PromotionBaseRepository<Voucher, Guid>(dbContext), IVoucherRepository
{
}
