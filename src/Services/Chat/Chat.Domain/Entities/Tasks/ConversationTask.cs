namespace NovaCore.Chat.Domain.Entities.Tasks;

/// <summary>Lightweight work item attached to a conversation - not a Message (spec section 32). Not a full project-management system.</summary>
public sealed class ConversationTask : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ConversationTaskStatus Status { get; private set; }
    public ConversationTaskPriority Priority { get; private set; }
    public Guid? AssigneeUserId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? DueAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private ConversationTask() { }

    public static ConversationTask Create(
        Guid conversationId,
        string title,
        Guid createdByUserId,
        string? description = null,
        ConversationTaskPriority priority = ConversationTaskPriority.Normal,
        Guid? assigneeUserId = null,
        DateTime? dueAt = null,
        ChatMetadata? metadata = null)
    {
        ValidateTitle(title);

        return new ConversationTask
        {
            Id = Guid.CreateVersion7(),
            ConversationId = conversationId,
            Title = title,
            Description = description,
            Status = ConversationTaskStatus.Todo,
            Priority = priority,
            AssigneeUserId = assigneeUserId,
            CreatedByUserId = createdByUserId,
            DueAt = dueAt,
            Metadata = metadata,
        };
    }
    #endregion

    #region Details & assignment
    public void UpdateDetails(string title, string? description, DateTime? dueAt)
    {
        ValidateTitle(title);

        Title = title;
        Description = description;
        DueAt = dueAt;
    }

    public void ChangePriority(ConversationTaskPriority priority)
    {
        Priority = priority;
    }

    public void AssignTo(Guid? assigneeUserId)
    {
        AssigneeUserId = assigneeUserId;
    }

    public static bool IsValidTitle(string? title) => !string.IsNullOrWhiteSpace(title);

    private static void ValidateTitle(string title)
    {
        if (!IsValidTitle(title))
            throw ExceptionFactory.RequiredField("Task title cannot be empty.");
    }
    #endregion

    #region Status
    public void Start()
    {
        if (Status != ConversationTaskStatus.Todo)
            throw ExceptionFactory.InvalidStatus($"Cannot start a task in {Status} status.");

        Status = ConversationTaskStatus.InProgress;
    }

    public void Complete()
    {
        if (Status is not (ConversationTaskStatus.Todo or ConversationTaskStatus.InProgress))
            throw ExceptionFactory.InvalidStatus($"Cannot complete a task in {Status} status.");

        Status = ConversationTaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is ConversationTaskStatus.Completed or ConversationTaskStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a task in {Status} status.");

        Status = ConversationTaskStatus.Cancelled;
    }
    #endregion
}
