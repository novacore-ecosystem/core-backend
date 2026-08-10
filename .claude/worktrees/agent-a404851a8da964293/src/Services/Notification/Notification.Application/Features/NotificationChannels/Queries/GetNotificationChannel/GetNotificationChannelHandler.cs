using NovaCore.Notification.Application.Abstractions.Persistence.NotificationChannels;

using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Notification.Application.Features.NotificationChannels.Queries.GetNotificationChannel;

public sealed class GetNotificationChannelHandler(INotificationChannelReadService notificationChannelReadService)
    : IQueryHandler<GetNotificationChannelQuery, GetNotificationChannelResponse>
{
    public async Task<GetNotificationChannelResponse> Handle(GetNotificationChannelQuery request, CancellationToken ct = default)
    {
        var entity = await notificationChannelReadService.GetByIdAsync(request.ChannelId, ct)
            ?? throw new NotFoundException("NotificationChannel", request.ChannelId);

        return new GetNotificationChannelResponse(
            entity.Id,
            entity.ChannelType,
            entity.DisplayName,
            entity.Status,
            entity.Configuration.ConfigJson,
            entity.ValidationStatus,
            entity.LastValidatedAt,
            entity.LastValidationError);
    }
}
