namespace NovaCore.BuildingBlock.Application.Abstractions.Jobs;

public interface IScheduledJobScheduler
{
    string Schedule<TJob>(TimeSpan delay) where TJob : IScheduledJob;
}
