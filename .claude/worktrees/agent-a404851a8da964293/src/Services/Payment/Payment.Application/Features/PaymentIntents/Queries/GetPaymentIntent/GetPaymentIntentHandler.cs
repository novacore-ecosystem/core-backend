using NovaCore.Payment.Application.Abstractions.Persistence.PaymentIntents;

namespace NovaCore.Payment.Application.Features.PaymentIntents.Queries.GetPaymentIntent;

public sealed class GetPaymentIntentHandler(IPaymentIntentReadService paymentIntentReadService) : IQueryHandler<GetPaymentIntentQuery, GetPaymentIntentResponse>
{
    public async Task<GetPaymentIntentResponse> Handle(GetPaymentIntentQuery request, CancellationToken ct = default)
    {
        var intent = await paymentIntentReadService.GetByIdAsync(request.PaymentIntentId, ct)
            ?? throw new NotFoundException(nameof(PaymentIntent), request.PaymentIntentId);

        return new GetPaymentIntentResponse(
            intent.Id,
            intent.ReferenceType,
            intent.ReferenceId,
            intent.RequestedAmount.Amount,
            intent.RequestedAmount.Currency.Value,
            intent.Status,
            intent.ClientSecret,
            intent.ExpiresAt,
            intent.CreatedAt);
    }
}
