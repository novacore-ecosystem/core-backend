using FluentValidation;

using Microsoft.Extensions.Options;

namespace NovaCore.BuildingBlock.Infrastructure.Configurations;

/// <summary>Adapts a manually-instantiated FluentValidation validator into the options-validation pipeline that <c>ValidateOnStart()</c> triggers, without registering the validator itself in DI.</summary>
internal sealed class FluentValidationOptions<TSetting>(IValidator<TSetting> validator) : IValidateOptions<TSetting>
    where TSetting : class
{
    public ValidateOptionsResult Validate(string? name, TSetting options)
    {
        var result = validator.Validate(options);

        return result.IsValid
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(result.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}"));
    }
}
