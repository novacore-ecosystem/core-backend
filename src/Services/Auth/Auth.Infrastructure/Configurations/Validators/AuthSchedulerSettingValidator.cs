using NovaCore.Auth.Infrastructure.Configurations.Settings;

using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs;

using FluentValidation;

namespace NovaCore.Auth.Infrastructure.Configurations.Validators;

/// <summary>Inherits the baseline scheduling rules and adds checks for the batch-size knobs this job alone declares.</summary>
public sealed class AuthSchedulerSettingValidator : SchedulerSettingBaseValidator<AuthSchedulerSetting>
{
    public AuthSchedulerSettingValidator()
    {
        RuleFor(x => x.UserBatchSize).GreaterThan(0).WithMessage("UserBatchSize must be positive.");
        RuleFor(x => x.TokenBatchSize).GreaterThan(0).WithMessage("TokenBatchSize must be positive.");
    }
}
