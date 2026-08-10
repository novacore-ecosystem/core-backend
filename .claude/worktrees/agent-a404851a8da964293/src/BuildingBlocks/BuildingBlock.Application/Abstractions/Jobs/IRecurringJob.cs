using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.BuildingBlock.Application.Abstractions.Jobs;

public interface IRecurringJob
{
    string JobId { get; }

    string CronExpression { get; }

    string Queue => JobQueueConstant.DEFAULT;

    bool IsInit { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}
