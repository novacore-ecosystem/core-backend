namespace NovaCore.Notification.Domain.ValueObjects;

/// <summary>
/// Raw, unrendered template copy for one <see cref="Entities.NotificationTemplate"/> - Body holds
/// placeholders (e.g. "Hi {{customerName}}, your order {{orderId}} shipped"), rendered later by
/// whichever worker actually dispatches. Subject is optional since not every channel has one
/// (Email/Telegram messages do, Push/SignalR payloads typically don't).
/// </summary>
public sealed class TemplateContent : ValueObject
{
    public string? Subject { get; }
    public string Body { get; }
    public IReadOnlyCollection<string> Variables { get; }

    private TemplateContent(string? subject, string body, IReadOnlyCollection<string> variables)
    {
        Subject = subject;
        Body = body;
        Variables = variables;
    }

    public static bool IsValid(string? body) => GetValidationError(body) is null;

    public static bool TryCreate(string? subject, string? body, IEnumerable<string>? variables, out TemplateContent? content)
    {
        if (GetValidationError(body) is not null)
        {
            content = null;
            return false;
        }

        content = new TemplateContent(NormalizeSubject(subject), body!.Trim(), NormalizeVariables(variables));
        return true;
    }

    public static TemplateContent Create(string? subject, string body, IEnumerable<string>? variables = null)
    {
        var error = GetValidationError(body);
        if (error is not null)
            throw error;

        return new TemplateContent(NormalizeSubject(subject), body.Trim(), NormalizeVariables(variables));
    }

    private static string? NormalizeSubject(string? subject) =>
        string.IsNullOrWhiteSpace(subject) ? null : subject.Trim();

    private static IReadOnlyCollection<string> NormalizeVariables(IEnumerable<string>? variables) =>
        variables is null
            ? []
            : [.. variables.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Distinct()];

    private static InvalidArgumentException? GetValidationError(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return ExceptionFactory.RequiredField("Template body cannot be empty.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Subject ?? string.Empty;
        yield return Body;
        foreach (var variable in Variables.OrderBy(v => v, StringComparer.Ordinal))
            yield return variable;
    }
}
