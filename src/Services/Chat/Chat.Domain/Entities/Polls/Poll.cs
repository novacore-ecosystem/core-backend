namespace NovaCore.Chat.Domain.Entities.Polls;

/// <summary>A conversation capability, not an ordinary text message (spec section 31) - MessageId optionally links it to the Message that announced it.</summary>
public sealed class Poll : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Guid? MessageId { get; private set; }
    public string Question { get; private set; } = string.Empty;
    public PollStatus Status { get; private set; }
    public bool MultipleChoice { get; private set; }
    public bool Anonymous { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public ICollection<PollOption> Options { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Poll() { }

    public static Poll Create(
        Guid conversationId,
        string question,
        Guid createdByUserId,
        bool multipleChoice = false,
        bool anonymous = false,
        Guid? messageId = null,
        DateTime? expiresAt = null,
        ChatMetadata? metadata = null)
    {
        ValidateQuestion(question);

        return new Poll
        {
            Id = Guid.CreateVersion7(),
            ConversationId = conversationId,
            MessageId = messageId,
            Question = question,
            Status = PollStatus.Active,
            MultipleChoice = multipleChoice,
            Anonymous = anonymous,
            ExpiresAt = expiresAt,
            CreatedByUserId = createdByUserId,
            Metadata = metadata,
        };
    }
    #endregion

    #region Options
    public void AddOption(string content)
    {
        Options.Add(PollOption.Create(Id, content, Options.Count));
    }

    public void RemoveOption(Guid optionId)
    {
        var option = Options.FirstOrDefault(o => o.Id == optionId)
            ?? throw ExceptionFactory.EntityNotFound<PollOption>(optionId);

        Options.Remove(option);
    }
    #endregion

    #region Status
    public void Close()
    {
        if (Status != PollStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot close a poll in {Status} status.");

        Status = PollStatus.Closed;
    }

    public void Cancel()
    {
        if (Status != PollStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a poll in {Status} status.");

        Status = PollStatus.Cancelled;
    }

    public static bool IsValidQuestion(string? question) => !string.IsNullOrWhiteSpace(question);

    private static void ValidateQuestion(string question)
    {
        if (!IsValidQuestion(question))
            throw ExceptionFactory.RequiredField("Poll question cannot be empty.");
    }
    #endregion
}
