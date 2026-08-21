namespace NovaCore.Chat.Domain.Entities.Schedules;

/// <summary>A scheduled activity/message attached to a conversation - the scheduler engine that actually executes these belongs to Application/Infrastructure (spec section 33).</summary>
public sealed class ConversationSchedule : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public ConversationScheduleType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Content { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public ConversationScheduleStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? ExecutedAt { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private ConversationSchedule() { }

    public static ConversationSchedule Create(
        Guid conversationId,
        ConversationScheduleType type,
        string title,
        DateTime scheduledAt,
        Guid createdByUserId,
        string? content = null,
        ChatMetadata? metadata = null)
    {
        ValidateTitle(title);

        return new ConversationSchedule
        {
            Id = Guid.CreateVersion7(),
            ConversationId = conversationId,
            Type = type,
            Title = title,
            Content = content,
            ScheduledAt = scheduledAt,
            Status = ConversationScheduleStatus.Scheduled,
            CreatedByUserId = createdByUserId,
            Metadata = metadata,
        };
    }
    #endregion

    #region Status
    public void MarkExecuted()
    {
        if (Status != ConversationScheduleStatus.Scheduled)
            throw ExceptionFactory.InvalidStatus($"Cannot execute a schedule in {Status} status.");

        Status = ConversationScheduleStatus.Executed;
        ExecutedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        if (Status != ConversationScheduleStatus.Scheduled)
            throw ExceptionFactory.InvalidStatus($"Cannot fail a schedule in {Status} status.");

        Status = ConversationScheduleStatus.Failed;
    }

    public void Cancel()
    {
        if (Status != ConversationScheduleStatus.Scheduled)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a schedule in {Status} status.");

        Status = ConversationScheduleStatus.Cancelled;
    }

    public static bool IsValidTitle(string? title) => !string.IsNullOrWhiteSpace(title);

    private static void ValidateTitle(string title)
    {
        if (!IsValidTitle(title))
            throw ExceptionFactory.RequiredField("Schedule title cannot be empty.");
    }
    #endregion
}
