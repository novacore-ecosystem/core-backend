namespace NovaCore.Chat.Infrastructure.Security;

/// <summary>Role claim value stamped on a guest's short-lived access token (see GuestTokenGenerator) and checked by ChatHub - local to Chat, not added to the platform-wide AppRoleConstant since no other service has a concept of a guest identity.</summary>
public static class ChatRoleConstants
{
    public const string Guest = "Guest";
}
