using NovaCore.Promotion.Application.Abstractions.Persistence.Vouchers;
using NovaCore.Promotion.Persistence.Contexts.Vouchers.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Vouchers.Read;

public sealed class VoucherReadService(IVoucherRepository voucherRepo) : IVoucherReadService
{
}
