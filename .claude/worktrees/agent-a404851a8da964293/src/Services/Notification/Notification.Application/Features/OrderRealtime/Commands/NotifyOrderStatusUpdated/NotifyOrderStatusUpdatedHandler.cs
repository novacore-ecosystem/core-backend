using NovaCore.Notification.Application.Abstractions.Services;
using NovaCore.Notification.Application.Features.UserNotifications.DTOs;

namespace NovaCore.Notification.Application.Features.OrderRealtime.Commands.NotifyOrderStatusUpdated;

public sealed class NotifyOrderStatusUpdatedHandler(IRealtimeNotifier realtimeNotifier)
    : ICommandHandler<NotifyOrderStatusUpdatedCommand>
{
    public async Task Handle(NotifyOrderStatusUpdatedCommand request, CancellationToken ct = default)
    {
        var dto = new OrderStatusUpdatedDto(
            request.OrderId,
            request.CustomerId,
            request.Status,
            request.Reason,
            request.TotalAmount,
            DateTime.UtcNow);

        await realtimeNotifier.PushOrderStatusUpdatedAsync(request.CustomerId, dto, ct);
    }
}
