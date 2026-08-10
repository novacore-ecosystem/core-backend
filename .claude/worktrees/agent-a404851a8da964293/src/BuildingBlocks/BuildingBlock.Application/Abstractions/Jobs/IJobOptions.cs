namespace NovaCore.BuildingBlock.Application.Abstractions.Jobs;

public interface IJobOptions
{
    string JobId { get; set; }
    string CronExpression { get; set; }
    string Queue { get; set; }
    bool IsInit { get; set; }
}
