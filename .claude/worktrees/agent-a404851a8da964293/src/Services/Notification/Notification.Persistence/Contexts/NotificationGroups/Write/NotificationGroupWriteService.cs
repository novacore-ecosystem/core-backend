using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Notification.Application.Abstractions.Persistence.NotificationGroups;
using NovaCore.Notification.Persistence.Contexts.NotificationGroups.Repositories;

namespace NovaCore.Notification.Persistence.Contexts.NotificationGroups.Write;

public sealed class NotificationGroupWriteService(
    INotificationGroupRepository repo,
    IUnitOfWork unitOfWork) : INotificationGroupWriteService
{
    public async Task CreateAsync(NotificationGroup entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
