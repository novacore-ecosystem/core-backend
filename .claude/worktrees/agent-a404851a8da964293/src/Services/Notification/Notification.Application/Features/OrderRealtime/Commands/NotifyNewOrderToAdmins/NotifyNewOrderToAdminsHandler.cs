using NovaCore.Notification.Application.Abstractions.Services;
using NovaCore.Notification.Application.Features.UserNotifications.DTOs;

namespace NovaCore.Notification.Application.Features.OrderRealtime.Commands.NotifyNewOrderToAdmins;

public sealed class NotifyNewOrderToAdminsHandler(IRealtimeNotifier realtimeNotifier)
    : ICommandHandler<NotifyNewOrderToAdminsCommand>
{
    public async Task Handle(NotifyNewOrderToAdminsCommand request, CancellationToken ct = default)
    {
        var dto = new NewOrderNotificationDto(
            request.OrderId,
            request.CustomerId,
            request.TotalAmount,
            DateTime.UtcNow);

        await realtimeNotifier.PushNewOrderToAdminsAsync(dto, ct);
    }
}
