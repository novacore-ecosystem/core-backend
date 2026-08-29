using NovaCore.Notification.Infrastructure.Configurations.Settings;

using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs;

using FluentValidation;

namespace NovaCore.Notification.Infrastructure.Configurations.Validators;

/// <summary>Inherits the baseline scheduling rules and adds checks for the batch/retry knobs this job alone declares.</summary>
public sealed class NotificationSchedulerSettingValidator : SchedulerSettingBaseValidator<NotificationSchedulerSetting>
{
    public NotificationSchedulerSettingValidator()
    {
        RuleFor(x => x.BatchSize).GreaterThan(0).WithMessage("BatchSize must be positive.");
        RuleFor(x => x.MaxRetryCount).GreaterThan(0).WithMessage("MaxRetryCount must be positive.");
    }
}
