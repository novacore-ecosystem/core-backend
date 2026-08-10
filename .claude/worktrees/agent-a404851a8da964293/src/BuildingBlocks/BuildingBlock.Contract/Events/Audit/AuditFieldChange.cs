namespace NovaCore.BuildingBlock.Contract.Events.Audit;

/// <summary>One property's before/after values, already string-formatted by the change-tracking pipeline.</summary>
public sealed record AuditFieldChange(
    string PropertyName,
    string? OldValue,
    string? NewValue);
