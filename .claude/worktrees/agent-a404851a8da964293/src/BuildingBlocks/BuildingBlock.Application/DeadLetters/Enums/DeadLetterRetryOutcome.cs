namespace NovaCore.BuildingBlock.Application.DeadLetters.Enums;

public enum DeadLetterRetryOutcome : byte
{
    /// <summary>The row was successfully requeued and republished.</summary>
    Succeeded,
    /// <summary>The row was not found in the Inbox table when the retry was attempted.</summary>
    NotFound,
    /// <summary>The row was not in the DeadLetter state (e.g. already requeued or deleted) when the retry was attempted.</summary>
    NotDeadLetter,
    /// <summary>Another retry for the same row is already in flight (distributed lock not acquired).</summary>
    Conflict,
    /// <summary>Row was requeued but the republish to Kafka itself failed; reverted back to DeadLetter.</summary>
    PublishFailed,
}
