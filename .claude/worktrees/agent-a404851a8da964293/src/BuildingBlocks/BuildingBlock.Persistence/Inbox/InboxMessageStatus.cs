namespace NovaCore.BuildingBlock.Persistence.Inbox;

/// <summary>
/// Lifecycle of an Inbox dedup/processing row. DeadLetter is a terminal status on the same
/// row - there is no separate DeadLetter table.
/// </summary>
public enum InboxMessageStatus
{
    Pending = 0,
    Retrying = 1,
    Processed = 2,
    DeadLetter = 3,
}
