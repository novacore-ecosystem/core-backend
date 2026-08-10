using MimeKit;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Internal;

internal static class MailKitExtensions
{
    public static MailboxAddress ToMailboxAddress(this EmailAddress address) =>
        new(address.Name, address.Address);

    public static IEnumerable<MailboxAddress> ToMailboxAddresses(this IEnumerable<EmailAddress> addresses) =>
        addresses.Select(address => address.ToMailboxAddress());

    public static MessagePriority ToMessagePriority(this EmailPriority priority) => priority switch
    {
        EmailPriority.Low => MessagePriority.NonUrgent,
        EmailPriority.High => MessagePriority.Urgent,
        _ => MessagePriority.Normal,
    };
}
