using System.Globalization;
using System.Text;

namespace NovaCore.Notification.Application.Features.UserNotifications.Queries.ListMyUserNotifications;

/// <summary>
/// Opaque pagination cursor for <see cref="ListMyUserNotificationsQuery"/> - base64 of
/// "{CreatedAt:O}|{Id}", matching the CreatedAt-desc/Id-desc sort
/// IUserNotificationRepository.GetMineAsync queries by. An unparsable cursor decodes to null
/// (treated as "first page") rather than throwing - callers shouldn't be able to 500 the endpoint
/// by tampering with an opaque token.
/// </summary>
internal static class UserNotificationCursor
{
    public static string Encode(DateTime createdAt, Guid id) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{createdAt:O}|{id}"));

    public static (DateTime CreatedAt, Guid Id)? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split('|', 2);
            if (parts.Length != 2)
                return null;

            var createdAt = DateTime.Parse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var id = Guid.Parse(parts[1]);
            return (createdAt, id);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
