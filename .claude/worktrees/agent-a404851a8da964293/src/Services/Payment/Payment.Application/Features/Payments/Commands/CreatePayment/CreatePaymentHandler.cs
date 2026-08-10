using NovaCore.Payment.Application.Abstractions.Persistence.Payments;

namespace NovaCore.Payment.Application.Features.Payments.Commands.CreatePayment;

public sealed class CreatePaymentHandler(
    IPaymentReadService paymentReadService,
    IPaymentWriteService paymentWriteService,
    IUnitOfWork uow) : ICommandHandler<CreatePaymentCommand, CreatePaymentResponse>
{
    public async Task<CreatePaymentResponse> Handle(CreatePaymentCommand request, CancellationToken ct = default)
    {
        // A payment create request is expected to be retried (network timeouts, client retries) -
        // replaying with the same IdempotencyKey returns the original Payment instead of creating
        // a duplicate charge.
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await paymentReadService.GetByIdempotencyKeyAsync(request.IdempotencyKey, ct);
            if (existing is not null)
                return new CreatePaymentResponse(existing.Id, existing.Status);
        }

        var amount = Money.Create(request.Amount, request.CurrencyCode);

        PaymentEntity payment = null!;

        await uow.ExecuteTransactionAsync(async () =>
        {
            payment = await paymentWriteService.CreateAsync(
                request.ReferenceType,
                request.ReferenceId,
                amount,
                request.GatewayId,
                request.PaymentIntentId,
                request.PaymentMethodId,
                request.IdempotencyKey,
                request.ExpiresAt,
                request.Metadata,
                ct);
        }, ct: ct);

        return new CreatePaymentResponse(payment.Id, payment.Status);
    }
}
