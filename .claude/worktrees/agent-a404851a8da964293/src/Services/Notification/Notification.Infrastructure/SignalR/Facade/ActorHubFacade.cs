using Microsoft.AspNetCore.SignalR;

using NovaCore.Notification.Infrastructure.SignalR.Groups;

namespace NovaCore.Notification.Infrastructure.SignalR.Facade;

public sealed class ActorHubFacade<THub, TRoot, TSite>(IHubContext<THub, TRoot> hub)
    where THub : Hub<TRoot>
    where TRoot : class, TSite
    where TSite : class
{
    public TSite All() => hub.Clients.All;

    public TSite Root(Guid userId) => hub.Clients.Group(ActorGroups.Root(userId));
    public TSite AdminAll() => hub.Clients.Group(ActorGroups.Broadcast(AppRoleConstant.Admin));
    public TSite MemberAll() => hub.Clients.Group(ActorGroups.Broadcast(AppRoleConstant.User));
    public TSite Admin(Guid userId) => hub.Clients.Group(ActorGroups.Admin(userId));
    public TSite Member(Guid userId) => hub.Clients.Group(ActorGroups.Member(userId));
}
