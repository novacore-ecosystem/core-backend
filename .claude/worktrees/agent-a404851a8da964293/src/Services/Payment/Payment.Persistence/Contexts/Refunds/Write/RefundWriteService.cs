using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Payment.Application.Abstractions.Persistence.Refunds;
using NovaCore.Payment.Persistence.Contexts.Payments.Repositories;
using NovaCore.Payment.Persistence.Contexts.Refunds.Repositories;

namespace NovaCore.Payment.Persistence.Contexts.Refunds.Write;

public sealed class RefundWriteService(IRefundRepository refundRepo, IPaymentRepository paymentRepo) : IRefundWriteService
{
    public async Task<Refund> CreateAsync(Guid paymentId, Money amount, string reason, CancellationToken ct = default)
    {
        var payment = await paymentRepo.GetByIdAsync(paymentId, ct)
            ?? throw new NotFoundException(nameof(PaymentEntity), paymentId);

        var refund = Refund.Create(payment.Id, payment.ReferenceType, payment.ReferenceId, amount, reason);

        await refundRepo.AddAsync(refund, ct);

        return refund;
    }
}
