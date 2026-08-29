using NovaCore.Content.Infrastructure.Configurations.Settings;

using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs;

using FluentValidation;

namespace NovaCore.Content.Infrastructure.Configurations.Validators;

/// <summary>Inherits the baseline scheduling rules and adds checks for the batch-size knobs this job alone declares.</summary>
public sealed class ContentSchedulerSettingValidator : SchedulerSettingBaseValidator<ContentSchedulerSetting>
{
    public ContentSchedulerSettingValidator()
    {
        RuleFor(x => x.RetentionDays).GreaterThan(0).WithMessage("RetentionDays must be positive.");
        RuleFor(x => x.BatchSize).GreaterThan(0).WithMessage("BatchSize must be positive.");
    }
}
