using NovaCore.Payment.Application.Abstractions.Persistence.Refunds;

namespace NovaCore.Payment.Application.Features.Refunds.Commands.CreateRefund;

public sealed class CreateRefundHandler(
    IRefundWriteService refundWriteService,
    IUnitOfWork uow) : ICommandHandler<CreateRefundCommand, CreateRefundResponse>
{
    public async Task<CreateRefundResponse> Handle(CreateRefundCommand request, CancellationToken ct = default)
    {
        var amount = Money.Create(request.Amount, request.CurrencyCode);

        Refund refund = null!;

        await uow.ExecuteTransactionAsync(async () =>
        {
            refund = await refundWriteService.CreateAsync(request.PaymentId, amount, request.Reason, ct);
        }, ct: ct);

        return new CreateRefundResponse(refund.Id, refund.Status);
    }
}
