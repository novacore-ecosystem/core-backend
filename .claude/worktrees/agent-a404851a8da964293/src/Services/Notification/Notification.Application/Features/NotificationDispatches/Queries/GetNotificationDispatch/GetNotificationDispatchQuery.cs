namespace NovaCore.Notification.Application.Features.NotificationDispatches.Queries.GetNotificationDispatch;

public sealed record GetNotificationDispatchQuery(Guid DispatchId) : IQuery<GetNotificationDispatchResponse>;

public sealed record GetNotificationDispatchResponse(
    Guid Id,
    string ReferenceType,
    string ReferenceId,
    NotificationChannelType Channel,
    Guid? TemplateId,
    string Payload,
    DispatchStatus Status,
    int RetryCount,
    DateTime? NextRetryAt,
    string? LastError,
    DateTime? DispatchedAt,
    DateTime CreatedAt);
