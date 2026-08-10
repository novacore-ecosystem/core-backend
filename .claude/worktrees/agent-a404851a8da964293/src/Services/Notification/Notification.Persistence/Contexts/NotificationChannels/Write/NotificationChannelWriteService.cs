using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Notification.Application.Abstractions.Persistence.NotificationChannels;
using NovaCore.Notification.Persistence.Contexts.NotificationChannels.Repositories;

namespace NovaCore.Notification.Persistence.Contexts.NotificationChannels.Write;

public sealed class NotificationChannelWriteService(
    INotificationChannelRepository repo,
    IUnitOfWork unitOfWork) : INotificationChannelWriteService
{
    public async Task UpdateAsync(NotificationChannel entity, CancellationToken ct = default)
    {
        await repo.UpdateAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
