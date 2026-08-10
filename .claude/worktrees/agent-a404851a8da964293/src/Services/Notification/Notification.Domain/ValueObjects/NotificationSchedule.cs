namespace NovaCore.Notification.Domain.ValueObjects;

/// <summary>
/// Encapsulates a <see cref="Entities.NotificationCampaign"/>'s execution timing so scheduling
/// rules live in one place instead of being scattered across primitive fields on the aggregate.
/// Once-campaigns run a single time at <see cref="StartAt"/>; Recurring campaigns repeat per
/// <see cref="CronExpression"/> within the optional [<see cref="StartAt"/>, <see cref="EndAt"/>] window.
/// </summary>
public sealed class NotificationSchedule : ValueObject
{
    public CampaignExecutionType ExecutionType { get; }
    public DateTime StartAt { get; }
    public DateTime? EndAt { get; }
    public string? CronExpression { get; }

    private NotificationSchedule(CampaignExecutionType executionType, DateTime startAt, DateTime? endAt, string? cronExpression)
    {
        ExecutionType = executionType;
        StartAt = startAt;
        EndAt = endAt;
        CronExpression = cronExpression;
    }

    public static bool IsValid(CampaignExecutionType executionType, DateTime startAt, DateTime? endAt, string? cronExpression) =>
        GetValidationError(executionType, startAt, endAt, cronExpression) is null;

    public static bool TryCreate(
        CampaignExecutionType executionType,
        DateTime startAt,
        DateTime? endAt,
        string? cronExpression,
        out NotificationSchedule? schedule)
    {
        if (GetValidationError(executionType, startAt, endAt, cronExpression) is not null)
        {
            schedule = null;
            return false;
        }

        schedule = new NotificationSchedule(executionType, startAt, endAt, cronExpression?.Trim());
        return true;
    }

    public static NotificationSchedule Create(
        CampaignExecutionType executionType,
        DateTime startAt,
        DateTime? endAt = null,
        string? cronExpression = null)
    {
        var error = GetValidationError(executionType, startAt, endAt, cronExpression);
        if (error is not null)
            throw error;

        return new NotificationSchedule(executionType, startAt, endAt, cronExpression?.Trim());
    }

    /// <summary>Whether this schedule is currently eligible to run - used by campaign-execution logic to decide if "now" falls inside the configured window.</summary>
    public bool IsWithinWindow(DateTime asOfUtc) =>
        asOfUtc >= StartAt && (EndAt is null || asOfUtc <= EndAt.Value);

    private static InvalidArgumentException? GetValidationError(
        CampaignExecutionType executionType,
        DateTime startAt,
        DateTime? endAt,
        string? cronExpression)
    {
        if (endAt is not null && endAt.Value <= startAt)
            return ExceptionFactory.InvalidRange("Schedule end date must be after the start date.");

        if (executionType == CampaignExecutionType.Recurring && string.IsNullOrWhiteSpace(cronExpression))
            return ExceptionFactory.RequiredField("A recurring schedule requires a cron expression.");

        if (executionType == CampaignExecutionType.Once && !string.IsNullOrWhiteSpace(cronExpression))
            return ExceptionFactory.InvalidFormat("A one-time schedule cannot have a cron expression.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return ExecutionType;
        yield return StartAt;
        yield return EndAt ?? (object)string.Empty;
        yield return CronExpression ?? string.Empty;
    }
}
