namespace NovaCore.Notification.Domain.ValueObjects;

/// <summary>Title/body pair shown in the Notification Center. Deliberately separate from <see cref="TemplateContent"/> - this is rendered, final copy, not a template with placeholders.</summary>
public sealed class NotificationContent : ValueObject
{
    private const int MaxTitleLength = 200;

    public string Title { get; }
    public string Body { get; }

    private NotificationContent(string title, string body)
    {
        Title = title;
        Body = body;
    }

    public static bool IsValid(string? title, string? body) => GetValidationError(title, body) is null;

    public static bool TryCreate(string? title, string? body, out NotificationContent? content)
    {
        if (GetValidationError(title, body) is not null)
        {
            content = null;
            return false;
        }

        content = new NotificationContent(title!.Trim(), body!.Trim());
        return true;
    }

    public static NotificationContent Create(string title, string body)
    {
        var error = GetValidationError(title, body);
        if (error is not null)
            throw error;

        return new NotificationContent(title.Trim(), body.Trim());
    }

    private static InvalidArgumentException? GetValidationError(string? title, string? body)
    {
        if (string.IsNullOrWhiteSpace(title))
            return ExceptionFactory.RequiredField("Notification title cannot be empty.");

        if (title.Trim().Length > MaxTitleLength)
            return ExceptionFactory.ValueTooLarge($"Notification title cannot exceed {MaxTitleLength} characters.");

        if (string.IsNullOrWhiteSpace(body))
            return ExceptionFactory.RequiredField("Notification body cannot be empty.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Title;
        yield return Body;
    }
}
