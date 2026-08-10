using NovaCore.Notification.Application.Features.UserNotifications.DTOs;

namespace NovaCore.Notification.Infrastructure.SignalR.Hubs.Global;

public interface IClientSiteActions : IGlobalHubBase
{
    /// <summary>An order this customer placed reached Confirmed or Cancelled - pushed to that customer only (see ActorHubFacade.Member(userId)).</summary>
    Task OrderStatusUpdated(OrderStatusUpdatedDto message);
}

public partial class GlobalHub
{

}