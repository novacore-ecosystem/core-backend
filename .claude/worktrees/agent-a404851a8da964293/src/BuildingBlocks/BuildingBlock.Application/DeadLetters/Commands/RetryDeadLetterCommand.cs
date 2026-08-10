using NovaCore.BuildingBlock.Application.Abstractions.CQRS;
using NovaCore.BuildingBlock.Application.DeadLetters.Enums;
using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.BuildingBlock.Application.DeadLetters.Commands;

public sealed record RetryDeadLetterCommand(Guid Id) : ICommand<RetryDeadLetterResponse>;

public sealed record RetryDeadLetterResponse(Guid Id, string Outcome);

public sealed class RetryDeadLetterHandler(IDeadLetterRetryService retryService)
    : ICommandHandler<RetryDeadLetterCommand, RetryDeadLetterResponse>
{
    public async Task<RetryDeadLetterResponse> Handle(
        RetryDeadLetterCommand request,
        CancellationToken ct = default)
    {
        var result = await retryService.RetryAsync(request.Id, ct);

        return result.Outcome switch
        {
            DeadLetterRetryOutcome.Succeeded => new RetryDeadLetterResponse(request.Id, "Retrying"),
            DeadLetterRetryOutcome.NotFound => throw new NotFoundException("DeadLetterMessage", request.Id),
            DeadLetterRetryOutcome.NotDeadLetter =>
                throw new BadRequestException($"Dead-letter message {request.Id} is not currently in DeadLetter status."),
            DeadLetterRetryOutcome.Conflict =>
                throw new ConflictException($"Dead-letter message {request.Id} is already being retried."),
            DeadLetterRetryOutcome.PublishFailed =>
                throw new InvalidOperationException($"Failed to republish dead-letter message {request.Id}: {result.Error}"),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Outcome, null),
        };
    }
}
