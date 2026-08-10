using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Notification.Application.Abstractions.Persistence.NotificationRules;
using NovaCore.Notification.Persistence.Contexts.NotificationRules.Repositories;

namespace NovaCore.Notification.Persistence.Contexts.NotificationRules.Write;

public sealed class NotificationRuleWriteService(
    INotificationRuleRepository repo,
    IUnitOfWork unitOfWork) : INotificationRuleWriteService
{
    public async Task CreateAsync(NotificationRule entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
