using NovaCore.BuildingBlock.Application.Abstractions.CQRS;
using NovaCore.BuildingBlock.Application.Abstractions.DeadLetters;
using NovaCore.BuildingBlock.Application.DeadLetters.Enums;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.BuildingBlock.Application.DeadLetters.Commands;

/// <summary>
/// Retry every DeadLetter row matching an optional filter (e.g. "retry all failed Product
/// events" via a ConsumerName/Topic filter, or "retry all older than X" via a CreatedAt filter -
/// both expressed through the same CriteriaRequest the search API uses). Capped at
/// MaxBatchSize per call to avoid one request trying to republish an unbounded number of
/// messages; callers needing more must call again.
/// </summary>
public sealed record RetryAllDeadLettersCommand(
    CriteriaRequest? Filter) : ICommand<RetryDeadLettersSummary>;

public sealed class RetryAllDeadLettersHandler(
    IDeadLetterQueryService queryService,
    IDeadLetterRetryService retryService)
    : ICommandHandler<RetryAllDeadLettersCommand, RetryDeadLettersSummary>
{
    private const int MaxBatchSize = 500;

    public async Task<RetryDeadLettersSummary> Handle(
        RetryAllDeadLettersCommand request,
        CancellationToken ct = default)
    {
        var criteria = (request.Filter ?? new CriteriaRequest()) with
        {
            Page = 1,
            PageSize = MaxBatchSize
        };

        var eligible = await queryService.SearchAsync(criteria, ct);

        var succeeded = 0;
        var failed = 0;
        var skipped = new List<RetryDeadLettersSkip>();

        foreach (var item in eligible.Items)
        {
            var result = await retryService.RetryAsync(item.Id, ct);
            if (result.Outcome == DeadLetterRetryOutcome.Succeeded)
            {
                succeeded++;
            }
            else
            {
                failed++;
                var retryDeadLetterSkip = new RetryDeadLettersSkip(
                    item.Id,
                    result.Outcome.ToString());
                skipped.Add(retryDeadLetterSkip);
            }
        }

        return new RetryDeadLettersSummary(
            eligible.Items.Count(),
            succeeded,
            failed,
            skipped);
    }
}
