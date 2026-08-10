namespace NovaCore.Audit.Domain.Entities;

/// <summary>One property's before/after value, embedded inside an <see cref="AuditTrailNode"/>.</summary>
public sealed class AuditTrailFieldChange
{
    public string PropertyName { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }

    private AuditTrailFieldChange() { }

    public static AuditTrailFieldChange Create(string propertyName, string? oldValue, string? newValue)
    {
        return new AuditTrailFieldChange
        {
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue,
        };
    }
}
