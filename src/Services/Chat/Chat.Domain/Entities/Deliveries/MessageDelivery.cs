namespace NovaCore.Chat.Domain.Entities.Deliveries;

/// <summary>
/// Per-recipient delivery state for a message - composite key (MessageId, UserId). Independently
/// constructible, no navigation from Message (volume scales with participant count per message,
/// which Message intentionally does not own - see spec section 27/44, no delivery-attempt history).
/// </summary>
public sealed class MessageDelivery : BaseEntity, ITenantEntity
{
    public Guid MessageId { get; private set; }
    public Guid UserId { get; private set; }
    public MessageDeliveryStatus Status { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private MessageDelivery() { }

    public static MessageDelivery Create(Guid messageId, Guid userId)
    {
        return new MessageDelivery
        {
            MessageId = messageId,
            UserId = userId,
            Status = MessageDeliveryStatus.Pending,
        };
    }

    public void MarkDelivered()
    {
        Status = MessageDeliveryStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void MarkRead()
    {
        Status = MessageDeliveryStatus.Read;
        ReadAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = MessageDeliveryStatus.Failed;
    }
}
