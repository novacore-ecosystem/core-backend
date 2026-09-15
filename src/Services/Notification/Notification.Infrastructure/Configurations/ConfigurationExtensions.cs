using System.Reflection;

using NovaCore.Notification.Infrastructure.Configurations.Settings;
using NovaCore.Notification.Infrastructure.Configurations.Validators;

using NovaCore.BuildingBlock.Infrastructure.Configurations;
using NovaCore.BuildingBlock.Infrastructure.Mail.Options;
using NovaCore.BuildingBlock.Messaging.Kafka.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NovaCore.Notification.Infrastructure.Configurations;

/// <summary>Binds and validates every local <see cref="ISetting"/> in this assembly, then manually validates the external settings Notification depends on but doesn't own.</summary>
public static class ConfigurationExtensions
{
    public static IServiceCollection AddNotificationConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSettings(configuration, Assembly.GetExecutingAssembly());

        ValidateKafkaConfiguration(configuration);

        return services;
    }

    /// <summary>Binds and validates <see cref="ResendMailSetting"/>, mapped into the mail building block's own <see cref="ResendMailOptions"/> so Notification.Infrastructure never has to depend on the Resend SDK directly.</summary>
    public static ResendMailOptions GetValidatedResendMailOptions(IConfiguration configuration)
    {
        var setting = configuration.GetSection(ResendMailSetting.Section).Get<ResendMailSetting>() ?? new ResendMailSetting();
        var result = new ResendMailSettingValidator().Validate(setting);

        if (!result.IsValid)
            throw new OptionsValidationException(ResendMailSetting.Section, typeof(ResendMailSetting), result.Errors.Select(e => e.ErrorMessage));

        return new ResendMailOptions
        {
            ApiKey = setting.ApiKey,
            SenderEmail = setting.FromEmail,
            SenderName = setting.FromName,
        };
    }

    private static void ValidateKafkaConfiguration(IConfiguration configuration)
    {
        var kafkaOptions = configuration.GetSection(KafkaOptions.Section).Get<KafkaOptions>() ?? new KafkaOptions();
        var result = new KafkaOptionsValidator().Validate(kafkaOptions);

        if (!result.IsValid)
            throw new OptionsValidationException(KafkaOptions.Section, typeof(KafkaOptions), result.Errors.Select(e => e.ErrorMessage));
    }
}
