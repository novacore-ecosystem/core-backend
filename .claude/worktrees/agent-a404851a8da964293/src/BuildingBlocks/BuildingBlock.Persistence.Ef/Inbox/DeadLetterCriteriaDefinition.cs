using NovaCore.BuildingBlock.Criteria.Definition;

namespace NovaCore.BuildingBlock.Persistence.Ef.Inbox;

/// <summary>
/// Admin search field whitelist for dead-lettered InboxMessage rows. Filters only cover native
/// columns - MessageId/CorrelationId (which live inside HeadersJson) are not DB-filterable and
/// are exposed only via the detail endpoint. Callers must additionally constrain
/// `Status == DeadLetter` themselves; this definition doesn't do that (Status IS filterable here
/// only so a future "retrying vs dead-lettered" toggle isn't ruled out).
/// </summary>
public static class DeadLetterCriteriaDefinition
{
    public static readonly CriteriaDefinition<InboxMessage> Instance = CriteriaDefinition<InboxMessage>.Create()
        .Field(x => x.Id).Guid()
        .Field(x => x.MessageId).Guid()
        .Field(x => x.ConsumerName).String().Sortable().KeywordSearchable().IgnoreCase()
        .Field(x => x.Topic).String().Sortable().KeywordSearchable().IgnoreCase()
        .Field(x => x.Status).Enum().Sortable()
        .Field(x => x.RetryCount).Number().Sortable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Field(x => x.LastRetryAt).DateTime().Sortable()
        .Field(x => x.LastError).String().KeywordSearchable().IgnoreCase()
        .Build();
}
