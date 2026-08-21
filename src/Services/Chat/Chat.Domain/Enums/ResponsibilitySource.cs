namespace NovaCore.Chat.Domain.Enums;

/// <summary>Named ResponsibilitySource (spec calls it bare "Source") - too generic/collision-prone as a standalone type name.</summary>
public enum ResponsibilitySource : byte
{
    DirectAssignment = 1,
    Transfer = 2,
    Invitation = 3,
    QueueAssignment = 4,
}
