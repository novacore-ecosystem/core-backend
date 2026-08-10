namespace NovaCore.Payment.Domain.Enums;

/// <summary>Append-only business event catalog recorded on PaymentEventLog - never updated after being written.</summary>
public enum PaymentEventType : byte
{
    PaymentCreated = 1,
    PaymentAuthorized = 2,
    PaymentCaptured = 3,
    PaymentFailed = 4,
    PaymentExpired = 5,
    PaymentCanceled = 6,
    RefundRequested = 7,
    RefundSucceeded = 8,
    RefundFailed = 9,
}
