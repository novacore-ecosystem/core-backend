using NovaCore.Notification.Application.Features.UserNotifications.DTOs;

namespace NovaCore.Notification.Infrastructure.SignalR.Hubs.Global;

public interface IAdminSiteActions : IGlobalHubBase
{
    /// <summary>A new order needs approval - pushed to every connected admin (see ActorHubFacade.AdminAll()).</summary>
    Task OrderCreated(NewOrderNotificationDto message);
}

public partial class GlobalHub
{

}