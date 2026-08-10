using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Notification.Application.Abstractions.Persistence.NotificationCampaigns;
using NovaCore.Notification.Persistence.Contexts.NotificationCampaigns.Repositories;

namespace NovaCore.Notification.Persistence.Contexts.NotificationCampaigns.Write;

public sealed class NotificationCampaignWriteService(
    INotificationCampaignRepository repo,
    IUnitOfWork unitOfWork) : INotificationCampaignWriteService
{
    public async Task CreateAsync(NotificationCampaign entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
