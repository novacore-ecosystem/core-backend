namespace NovaCore.Payment.Domain.Entities.Scheduling;

/// <summary>Payment-related notification record (email/SMS/push) - tracks dispatch status only; actual sending is owned by NotificationService.</summary>
public sealed class PaymentNotification : AggregateRoot<Guid>, IAuditable
{
    public Guid? PaymentId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }

    private PaymentNotification() { }

    public static PaymentNotification Create(NotificationChannel channel, Guid? paymentId = null)
    {
        return new PaymentNotification
        {
            Id = Guid.CreateVersion7(),
            PaymentId = paymentId,
            Channel = channel,
            Status = NotificationStatus.Pending,
        };
    }
}
