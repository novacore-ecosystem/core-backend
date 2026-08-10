using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.BuildingBlock.Application.Abstractions.DeadLetters;

/// <summary>
/// Read-only search/detail surface over dead-lettered Inbox rows, always implicitly scoped to
/// Status == DeadLetter. Implemented once per storage provider (EF/Mongo) and registered per
/// service against its own DbContext/Mongo context - the same "generic, parameterized by
/// TContext" shape as IInboxStore's EfInboxStore&lt;TContext&gt;/MongoInboxStore&lt;TContext&gt;.
/// </summary>
public interface IDeadLetterQueryService
{
    Task<PaginatedResult<DeadLetterListItemResponse>> SearchAsync(CriteriaRequest request, CancellationToken ct = default);

    Task<DeadLetterDetailResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
