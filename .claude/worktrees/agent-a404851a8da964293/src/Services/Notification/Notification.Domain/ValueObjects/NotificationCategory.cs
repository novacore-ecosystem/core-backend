namespace NovaCore.Notification.Domain.ValueObjects;

/// <summary>
/// Free-form classification of a <see cref="Entities.UserNotification"/> (e.g. "Account", "Order",
/// "Payment"), assigned by whatever created the notification - a NotificationRule execution, a
/// direct API call, an integration-event consumer. Not a closed enum: there is no fixed, curated
/// set of categories owned by this service (no seed data, no filter UI, nothing enumerating them
/// today) - it grows as new notification-producing flows are added, same reasoning as
/// <see cref="Entities.NotificationRule.EventType"/>.
/// </summary>
public sealed class NotificationCategory : ValueObject
{
    private const int MaxLength = 100;

    public string Value { get; }

    private NotificationCategory(string value)
    {
        Value = value;
    }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out NotificationCategory? category)
    {
        if (GetValidationError(value) is not null)
        {
            category = null;
            return false;
        }

        category = new NotificationCategory(value!.Trim());
        return true;
    }

    public static NotificationCategory Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new NotificationCategory(value.Trim());
    }

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Notification category cannot be empty.");

        if (value.Trim().Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Notification category cannot exceed {MaxLength} characters.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
