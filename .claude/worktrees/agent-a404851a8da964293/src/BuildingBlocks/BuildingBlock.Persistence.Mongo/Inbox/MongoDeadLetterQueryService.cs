using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.DeadLetters;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Mongo.MongoContext;

using MongoDB.Driver;

using AppInboxMessageStatus = NovaCore.BuildingBlock.Application.Abstractions.Outbox.InboxMessageStatus;
using AppInboxRetryHistoryEntry = NovaCore.BuildingBlock.Application.Abstractions.Outbox.InboxRetryHistoryEntry;
using DomainInboxMessageStatus = NovaCore.BuildingBlock.Persistence.Inbox.InboxMessageStatus;
using DomainInboxRetryHistoryResult = NovaCore.BuildingBlock.Persistence.Inbox.InboxRetryHistoryResult;
using RequestSortDirection = NovaCore.BuildingBlock.Criteria.Requests.SortDirection;

namespace NovaCore.BuildingBlock.Persistence.Mongo.Inbox;

/// <summary>
/// Generic Mongo implementation of IDeadLetterQueryService. No CriteriaDefinition reuse here
/// (that pipeline is EF/IQueryable-specific) - filter/sort/page are applied manually against the
/// same field whitelist DeadLetterCriteriaDefinition documents for the EF side: ConsumerName,
/// Topic, RetryCount, CreatedAt, LastRetryAt, plus a keyword search across ConsumerName/Topic/LastError.
/// </summary>
public sealed class MongoDeadLetterQueryService<TContext>(TContext context) : IDeadLetterQueryService
    where TContext : MongoContextBase, IInboxMongoContext
{
    private readonly TContext _context = context;

    public async Task<PaginatedResult<DeadLetterListItemResponse>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        var filter = Builders<InboxDocument>.Filter.Eq(x => x.Status, DomainInboxMessageStatus.DeadLetter);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keywordFilter = Builders<InboxDocument>.Filter.Or(
                Builders<InboxDocument>.Filter.Regex(x => x.ConsumerName, new MongoDB.Bson.BsonRegularExpression(request.Keyword, "i")),
                Builders<InboxDocument>.Filter.Regex(x => x.Topic, new MongoDB.Bson.BsonRegularExpression(request.Keyword, "i")),
                Builders<InboxDocument>.Filter.Regex(x => x.LastError, new MongoDB.Bson.BsonRegularExpression(request.Keyword, "i")));
            filter &= keywordFilter;
        }

        foreach (var f in request.Filters)
        {
            if (f.Value is null)
                continue;

            filter &= f.Field.ToLowerInvariant() switch
            {
                "consumername" => Builders<InboxDocument>.Filter.Regex(x => x.ConsumerName, new MongoDB.Bson.BsonRegularExpression(f.Value.Value.GetString() ?? string.Empty, "i")),
                "topic" => Builders<InboxDocument>.Filter.Regex(x => x.Topic, new MongoDB.Bson.BsonRegularExpression(f.Value.Value.GetString() ?? string.Empty, "i")),
                "retrycount" => Builders<InboxDocument>.Filter.Eq(x => x.RetryCount, f.Value.Value.GetInt32()),
                _ => Builders<InboxDocument>.Filter.Empty,
            };
        }

        var totalCount = (int)await _context.InboxMessages.CountDocumentsAsync(filter, cancellationToken: ct);

        var sort = request.Sorts.FirstOrDefault();
        var findQuery = _context.InboxMessages.Find(filter);
        findQuery = sort?.Field.ToLowerInvariant() switch
        {
            "createdat" => sort.Direction == RequestSortDirection.Desc ? findQuery.SortByDescending(x => x.CreatedAt) : findQuery.SortBy(x => x.CreatedAt),
            "lastretryat" => sort.Direction == RequestSortDirection.Desc ? findQuery.SortByDescending(x => x.LastRetryAt) : findQuery.SortBy(x => x.LastRetryAt),
            _ => findQuery.SortByDescending(x => x.CreatedAt),
        };

        var docs = await findQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync(ct);

        var items = docs.Select(ToListItem).ToList();
        return PaginatedResult<DeadLetterListItemResponse>.Create(items, request.Page, request.PageSize, totalCount);
    }

    public async Task<DeadLetterDetailResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await _context.InboxMessages.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc is null)
            return null;

        var historyDocs = await _context.InboxRetryHistories
            .Find(h => h.InboxMessageId == id)
            .SortByDescending(h => h.StartedAt)
            .ToListAsync(ct);

        var history = historyDocs.Select(h => new AppInboxRetryHistoryEntry(
            h.Id, h.InboxMessageId, h.MessageId, h.ConsumerName, h.Topic, h.RetryNumber,
            h.StartedAt, h.FinishedAt, h.DurationMs, h.Operator, ToApplication(h.Result), h.Exception)).ToList();

        return new DeadLetterDetailResponse(
            doc.Id, doc.MessageId, doc.ConsumerName, doc.Topic,
            doc.Payload, doc.HeadersJson, ToApplication(doc.Status), doc.RetryCount,
            doc.CreatedAt, doc.ProcessedAt, doc.NextRetryAt, doc.LastRetryAt, doc.LastError,
            history);
    }

    private static DeadLetterListItemResponse ToListItem(InboxDocument m) => new(
        m.Id, m.MessageId, m.ConsumerName, m.Topic, ToApplication(m.Status), m.RetryCount, m.CreatedAt, m.LastRetryAt, m.LastError);

    private static AppInboxMessageStatus ToApplication(DomainInboxMessageStatus status) => status switch
    {
        DomainInboxMessageStatus.Pending => AppInboxMessageStatus.Pending,
        DomainInboxMessageStatus.Retrying => AppInboxMessageStatus.Retrying,
        DomainInboxMessageStatus.Processed => AppInboxMessageStatus.Processed,
        DomainInboxMessageStatus.DeadLetter => AppInboxMessageStatus.DeadLetter,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };

    private static NovaCore.BuildingBlock.Application.Abstractions.Outbox.InboxRetryHistoryResult ToApplication(DomainInboxRetryHistoryResult result) => result switch
    {
        DomainInboxRetryHistoryResult.Retrying => NovaCore.BuildingBlock.Application.Abstractions.Outbox.InboxRetryHistoryResult.Retrying,
        DomainInboxRetryHistoryResult.Succeeded => NovaCore.BuildingBlock.Application.Abstractions.Outbox.InboxRetryHistoryResult.Succeeded,
        DomainInboxRetryHistoryResult.FailedAgain => NovaCore.BuildingBlock.Application.Abstractions.Outbox.InboxRetryHistoryResult.FailedAgain,
        DomainInboxRetryHistoryResult.Cancelled => NovaCore.BuildingBlock.Application.Abstractions.Outbox.InboxRetryHistoryResult.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
    };
}
