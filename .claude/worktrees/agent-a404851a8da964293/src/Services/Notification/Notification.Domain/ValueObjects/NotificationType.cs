namespace NovaCore.Notification.Domain.ValueObjects;

/// <summary>
/// Free-form sub-classification of a <see cref="Entities.UserNotification"/> (e.g. "Welcome",
/// "OrderCancelled") - typically mirrors the triggering business event's name. Not a closed enum,
/// same reasoning as <see cref="NotificationCategory"/> and
/// <see cref="Entities.NotificationRule.EventType"/>: business event names are defined by every
/// producing service and evolve independently of this one.
/// </summary>
public sealed class NotificationType : ValueObject
{
    private const int MaxLength = 100;

    public string Value { get; }

    private NotificationType(string value)
    {
        Value = value;
    }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out NotificationType? type)
    {
        if (GetValidationError(value) is not null)
        {
            type = null;
            return false;
        }

        type = new NotificationType(value!.Trim());
        return true;
    }

    public static NotificationType Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new NotificationType(value.Trim());
    }

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Notification type cannot be empty.");

        if (value.Trim().Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Notification type cannot exceed {MaxLength} characters.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
