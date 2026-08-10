using NovaCore.Payment.Application.Abstractions.Persistence.Payments;

namespace NovaCore.Payment.Application.Features.Payments.Queries.GetPayment;

public sealed class GetPaymentHandler(IPaymentReadService paymentReadService) : IQueryHandler<GetPaymentQuery, GetPaymentResponse>
{
    public async Task<GetPaymentResponse> Handle(GetPaymentQuery request, CancellationToken ct = default)
    {
        var payment = await paymentReadService.GetByIdAsync(request.PaymentId, ct)
            ?? throw new NotFoundException(nameof(PaymentEntity), request.PaymentId);

        return new GetPaymentResponse(
            payment.Id,
            payment.ReferenceType,
            payment.ReferenceId,
            payment.Amount.Amount,
            payment.Amount.Currency.Value,
            payment.Status,
            payment.GatewayId,
            payment.PaymentMethodId,
            payment.PaymentIntentId,
            MapItems(payment.Items),
            MapAttempts(payment.Attempts),
            payment.CreatedAt,
            payment.UpdatedAt);
    }

    private static GetPaymentItemResponse[] MapItems(IEnumerable<PaymentItem> items)
        => [.. items.Select(i => new GetPaymentItemResponse(i.Id, i.ItemType, i.Description, i.Amount.Amount, i.Amount.Currency.Value, i.Quantity))];

    private static GetPaymentAttemptResponse[] MapAttempts(IEnumerable<PaymentAttempt> attempts)
        => [.. attempts.Select(a => new GetPaymentAttemptResponse(a.Id, a.AttemptNumber, a.Status, a.GatewayTransactionId))];
}
