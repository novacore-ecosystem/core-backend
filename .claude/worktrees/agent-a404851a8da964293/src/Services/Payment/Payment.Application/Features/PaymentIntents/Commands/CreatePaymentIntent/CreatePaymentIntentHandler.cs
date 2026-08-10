using NovaCore.Payment.Application.Abstractions.Persistence.PaymentIntents;

namespace NovaCore.Payment.Application.Features.PaymentIntents.Commands.CreatePaymentIntent;

public sealed class CreatePaymentIntentHandler(
    IPaymentIntentWriteService paymentIntentWriteService,
    IUnitOfWork uow) : ICommandHandler<CreatePaymentIntentCommand, CreatePaymentIntentResponse>
{
    public async Task<CreatePaymentIntentResponse> Handle(CreatePaymentIntentCommand request, CancellationToken ct = default)
    {
        var amount = Money.Create(request.RequestedAmount, request.CurrencyCode);

        PaymentIntent intent = null!;

        await uow.ExecuteTransactionAsync(async () =>
        {
            intent = await paymentIntentWriteService.CreateAsync(
                request.ReferenceType,
                request.ReferenceId,
                amount,
                request.ExpiresAt,
                request.Metadata,
                ct);
        }, ct: ct);

        return new CreatePaymentIntentResponse(intent.Id, intent.ClientSecret, intent.Status);
    }
}
