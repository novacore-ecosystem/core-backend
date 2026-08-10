using NovaCore.BuildingBlock.Application.Abstractions.CQRS;
using NovaCore.BuildingBlock.Application.Abstractions.DeadLetters;
using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.BuildingBlock.Application.DeadLetters.Queries;

public sealed record GetDeadLetterQuery(Guid Id) : IQuery<DeadLetterDetailResponse>;

public sealed class GetDeadLetterHandler(IDeadLetterQueryService queryService)
    : IQueryHandler<GetDeadLetterQuery, DeadLetterDetailResponse>
{
    public async Task<DeadLetterDetailResponse> Handle(GetDeadLetterQuery request, CancellationToken ct = default) =>
        await queryService.GetByIdAsync(request.Id, ct)
        ?? throw new NotFoundException("DeadLetterMessage", request.Id);
}
