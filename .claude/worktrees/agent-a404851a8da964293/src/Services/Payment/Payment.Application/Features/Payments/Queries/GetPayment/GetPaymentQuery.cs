namespace NovaCore.Payment.Application.Features.Payments.Queries.GetPayment;

public sealed record GetPaymentQuery(Guid PaymentId) : IQuery<GetPaymentResponse>;

public sealed record GetPaymentItemResponse(Guid Id, PaymentItemType ItemType, string Description, decimal Amount, string Currency, int Quantity);

public sealed record GetPaymentAttemptResponse(Guid Id, int AttemptNumber, PaymentAttemptStatus Status, string? GatewayTransactionId);

public sealed record GetPaymentResponse(
    Guid Id,
    ReferenceType ReferenceType,
    Guid ReferenceId,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    Guid GatewayId,
    Guid? PaymentMethodId,
    Guid? PaymentIntentId,
    IReadOnlyCollection<GetPaymentItemResponse> Items,
    IReadOnlyCollection<GetPaymentAttemptResponse> Attempts,
    DateTime CreatedAt,
    DateTime UpdatedAt);
