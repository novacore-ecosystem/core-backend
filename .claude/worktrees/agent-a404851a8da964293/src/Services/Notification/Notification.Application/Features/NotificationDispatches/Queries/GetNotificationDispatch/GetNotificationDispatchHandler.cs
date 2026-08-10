using NovaCore.Notification.Application.Abstractions.Persistence.NotificationDispatches;

using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Notification.Application.Features.NotificationDispatches.Queries.GetNotificationDispatch;

public sealed class GetNotificationDispatchHandler(INotificationDispatchReadService notificationDispatchReadService)
    : IQueryHandler<GetNotificationDispatchQuery, GetNotificationDispatchResponse>
{
    public async Task<GetNotificationDispatchResponse> Handle(GetNotificationDispatchQuery request, CancellationToken ct = default)
    {
        var entity = await notificationDispatchReadService.GetByIdAsync(request.DispatchId, ct)
            ?? throw new NotFoundException("NotificationDispatch", request.DispatchId);

        return new GetNotificationDispatchResponse(
            entity.Id,
            entity.Reference.ReferenceType,
            entity.Reference.ReferenceId,
            entity.Channel,
            entity.TemplateId,
            entity.Payload,
            entity.Status,
            entity.RetryCount,
            entity.NextRetryAt,
            entity.LastError,
            entity.DispatchedAt,
            entity.CreatedAt);
    }
}
