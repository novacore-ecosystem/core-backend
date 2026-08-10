namespace NovaCore.Notification.Domain.Entities;

/// <summary>
/// One notification delivery request waiting to be processed - the "Delivery" half of this
/// service (see UserNotification for the "Business"/in-app half). Behaves like an Outbox: a
/// worker (not implemented yet - see NovaCore.Notification.Infrastructure/Workers) polls Pending/due-for-
/// retry rows and attempts delivery through whichever <see cref="Channel"/>-specific sender it
/// resolves via <see cref="NotificationChannel"/>. InApp is rejected in <see cref="Create"/> since
/// in-app entries are written directly to <see cref="UserNotification"/>, never queued here.
/// </summary>
public sealed class NotificationDispatch : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public DispatchReference Reference { get; private set; } = null!;
    public NotificationChannelType Channel { get; private set; }
    public Guid? TemplateId { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public DispatchStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public string? LastError { get; private set; }
    public DateTime? DispatchedAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private NotificationDispatch() { }

    public static NotificationDispatch Create(
        Guid id,
        DispatchReference reference,
        NotificationChannelType channel,
        string payload,
        Guid? templateId = null)
    {
        if (channel == NotificationChannelType.InApp)
            throw ExceptionFactory.InvalidState("InApp notifications are written directly to the Notification Center, not dispatched.");

        ValidatePayload(payload);

        return new NotificationDispatch
        {
            Id = id,
            Reference = reference,
            Channel = channel,
            TemplateId = templateId,
            Payload = payload,
            Status = DispatchStatus.Pending,
            RetryCount = 0,
        };
    }

    public void MarkProcessing()
    {
        if (Status != DispatchStatus.Pending && Status != DispatchStatus.Failed)
            throw ExceptionFactory.InvalidStatus($"Cannot start processing a dispatch in {Status} status.");

        Status = DispatchStatus.Processing;
    }

    public void MarkSent(DateTime sentAtUtc)
    {
        if (Status != DispatchStatus.Processing)
            throw ExceptionFactory.InvalidStatus($"Cannot mark a dispatch in {Status} status as sent.");

        Status = DispatchStatus.Sent;
        DispatchedAt = sentAtUtc;
        LastError = null;
        NextRetryAt = null;
    }

    /// <summary>Records a failed attempt. A null <paramref name="nextRetryAtUtc"/> means retries are exhausted and the dispatch goes straight to DeadLettered.</summary>
    public void MarkFailed(string error, DateTime? nextRetryAtUtc)
    {
        if (Status != DispatchStatus.Processing)
            throw ExceptionFactory.InvalidStatus($"Cannot fail a dispatch in {Status} status.");

        RetryCount++;
        LastError = error;

        if (nextRetryAtUtc is null)
        {
            Status = DispatchStatus.DeadLettered;
            NextRetryAt = null;
        }
        else
        {
            Status = DispatchStatus.Failed;
            NextRetryAt = nextRetryAtUtc;
        }
    }

    public bool IsDueForRetry(DateTime asOfUtc) =>
        Status == DispatchStatus.Failed && NextRetryAt is not null && asOfUtc >= NextRetryAt.Value;

    public static bool IsValidPayload(string? payload) => !string.IsNullOrWhiteSpace(payload);

    private static void ValidatePayload(string payload)
    {
        if (!IsValidPayload(payload))
            throw ExceptionFactory.RequiredField("Dispatch payload cannot be empty.");
    }
}
