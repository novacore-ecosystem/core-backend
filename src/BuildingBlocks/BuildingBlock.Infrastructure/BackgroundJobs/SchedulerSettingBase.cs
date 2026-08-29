using NovaCore.BuildingBlock.Application.Abstractions.Jobs;
using NovaCore.BuildingBlock.Infrastructure.Configurations;

namespace NovaCore.BuildingBlock.Infrastructure.BackgroundJobs;

/// <summary>Baseline Hangfire scheduling shape every recurring-job setting needs; subclasses override the virtual properties with their own job identity and add whatever extra knobs the job requires.</summary>
public abstract class SchedulerSettingBase : IJobOptions, ISetting
{
    public virtual string JobId { get; set; } = string.Empty;
    public virtual string CronExpression { get; set; } = "* * * * *";
    public virtual string Queue { get; set; } = "default";
    public virtual bool IsInit { get; set; } = true;
}
