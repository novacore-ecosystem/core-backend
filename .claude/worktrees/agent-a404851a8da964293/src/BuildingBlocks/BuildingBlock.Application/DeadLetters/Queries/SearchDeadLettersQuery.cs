using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.CQRS;
using NovaCore.BuildingBlock.Application.Abstractions.DeadLetters;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.BuildingBlock.Application.DeadLetters.Queries;

public sealed record SearchDeadLettersQuery(CriteriaRequest Criteria)
    : IQuery<PaginatedResult<DeadLetterListItemResponse>>;

public sealed class SearchDeadLettersHandler(IDeadLetterQueryService queryService)
    : IQueryHandler<SearchDeadLettersQuery, PaginatedResult<DeadLetterListItemResponse>>
{
    public Task<PaginatedResult<DeadLetterListItemResponse>> Handle(SearchDeadLettersQuery request, CancellationToken ct = default) =>
        queryService.SearchAsync(request.Criteria, ct);
}
