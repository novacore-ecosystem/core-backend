namespace NovaCore.Chat.Domain.Entities.Polls;

public sealed class PollOption : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid PollId { get; private set; }
    public Poll Poll { get; private set; } = default!;
    public string Content { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private PollOption() { }

    /// <summary>Only Poll may construct a PollOption - see Poll.AddOption.</summary>
    internal static PollOption Create(Guid pollId, string content, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw ExceptionFactory.RequiredField("Poll option content cannot be empty.");

        return new PollOption
        {
            Id = Guid.CreateVersion7(),
            PollId = pollId,
            Content = content,
            SortOrder = sortOrder,
        };
    }
}
