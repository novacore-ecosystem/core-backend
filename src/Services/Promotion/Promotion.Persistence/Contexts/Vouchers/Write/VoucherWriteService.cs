using NovaCore.Promotion.Application.Abstractions.Persistence.Vouchers;
using NovaCore.Promotion.Persistence.Contexts.Vouchers.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Vouchers.Write;

public sealed class VoucherWriteService(IVoucherRepository voucherRepo) : IVoucherWriteService
{
}
