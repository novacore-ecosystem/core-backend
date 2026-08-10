using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Notification.Application.Abstractions.Persistence.NotificationTemplates;
using NovaCore.Notification.Persistence.Contexts.NotificationTemplates.Repositories;

namespace NovaCore.Notification.Persistence.Contexts.NotificationTemplates.Write;

public sealed class NotificationTemplateWriteService(
    INotificationTemplateRepository repo,
    IUnitOfWork unitOfWork) : INotificationTemplateWriteService
{
    public async Task CreateAsync(NotificationTemplate entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
