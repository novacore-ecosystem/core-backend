using FluentValidation;

namespace NovaCore.BuildingBlock.Infrastructure.BackgroundJobs;

/// <summary>Baseline rules for <see cref="SchedulerSettingBase"/>. A service validator inherits this and adds RuleFor calls of its own for whatever extra properties its job needs.</summary>
public abstract class SchedulerSettingBaseValidator<TSetting> : AbstractValidator<TSetting>
    where TSetting : SchedulerSettingBase
{
    protected SchedulerSettingBaseValidator()
    {
        RuleFor(x => x.JobId).NotEmpty().WithMessage("JobId is required.");
        RuleFor(x => x.CronExpression).NotEmpty().WithMessage("CronExpression is required.");
        RuleFor(x => x.Queue).NotEmpty().WithMessage("Queue is required.");
    }
}
