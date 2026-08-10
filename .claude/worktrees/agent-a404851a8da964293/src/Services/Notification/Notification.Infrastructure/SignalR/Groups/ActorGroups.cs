namespace NovaCore.Notification.Infrastructure.SignalR.Groups;

internal static class ActorGroups
{
    public static string Root(Guid userId) => $"root:{userId}";
    public static string Admin(Guid userId) => $"admin:{userId}";
    public static string Member(Guid userId) => $"member:{userId}";

    public static string Broadcast(string roleName) => $"role:{roleName}";
}
