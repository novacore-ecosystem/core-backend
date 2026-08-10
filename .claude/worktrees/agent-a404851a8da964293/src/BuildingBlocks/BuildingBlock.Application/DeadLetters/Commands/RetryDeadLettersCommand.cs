using NovaCore.BuildingBlock.Application.Abstractions.CQRS;
using NovaCore.BuildingBlock.Application.DeadLetters.Enums;

namespace NovaCore.BuildingBlock.Application.DeadLetters.Commands;

/// <summary>
/// Retry a caller-supplied set of dead-lettered rows. Never processes more than one retry
/// implementation - every entry point (single/bulk/retry-all) funnels through IDeadLetterRetryService.
/// </summary>
public sealed record RetryDeadLettersCommand(
    IReadOnlyList<Guid> Ids) : ICommand<RetryDeadLettersSummary>;

public sealed record RetryDeadLettersSummary(
    int Requested,
    int Succeeded,
    int Failed,
    IReadOnlyList<RetryDeadLettersSkip> Skipped);

public sealed record RetryDeadLettersSkip(Guid Id, string Reason);

public sealed class RetryDeadLettersHandler(IDeadLetterRetryService retryService)
    : ICommandHandler<RetryDeadLettersCommand, RetryDeadLettersSummary>
{
    public async Task<RetryDeadLettersSummary> Handle(
        RetryDeadLettersCommand request,
        CancellationToken ct = default)
    {
        var succeeded = 0;
        var failed = 0;
        var skipped = new List<RetryDeadLettersSkip>();

        foreach (var id in request.Ids)
        {
            var result = await retryService.RetryAsync(id, ct);
            if (result.Outcome == DeadLetterRetryOutcome.Succeeded)
            {
                succeeded++;
            }
            else
            {
                failed++;
                var retryDeadLetterSkip = new RetryDeadLettersSkip(
                    id,
                    result.Outcome.ToString());
                skipped.Add(retryDeadLetterSkip);
            }
        }

        return new RetryDeadLettersSummary(
            request.Ids.Count,
            succeeded,
            failed,
            skipped);
    }
}
