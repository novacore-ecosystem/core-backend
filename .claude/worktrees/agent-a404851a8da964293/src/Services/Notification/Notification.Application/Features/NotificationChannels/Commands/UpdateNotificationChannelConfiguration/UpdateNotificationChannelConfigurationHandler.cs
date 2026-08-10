using NovaCore.Notification.Application.Abstractions.Persistence.NotificationChannels;
using NovaCore.Notification.Application.Abstractions.Services;

using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Notification.Application.Features.NotificationChannels.Commands.UpdateNotificationChannelConfiguration;

public sealed class UpdateNotificationChannelConfigurationHandler(
    INotificationChannelReadService notificationChannelReadService,
    INotificationChannelWriteService notificationChannelWriteService,
    INotificationChannelCache channelCache) : ICommandHandler<UpdateNotificationChannelConfigurationCommand>
{
    public async Task Handle(UpdateNotificationChannelConfigurationCommand request, CancellationToken ct = default)
    {
        var entity = await notificationChannelReadService.GetByIdAsync(request.ChannelId, ct)
            ?? throw new NotFoundException("NotificationChannel", request.ChannelId);

        var configuration = ChannelConfiguration.Create(request.ConfigJson);
        entity.UpdateConfiguration(configuration);

        await notificationChannelWriteService.UpdateAsync(entity, ct);
        await channelCache.InvalidateAsync(entity.ChannelType, ct);
    }
}
