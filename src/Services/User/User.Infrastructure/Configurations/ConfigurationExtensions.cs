using System.Reflection;

using NovaCore.User.Infrastructure.Configurations.Validators;

using NovaCore.BuildingBlock.Infrastructure.Configurations;
using NovaCore.BuildingBlock.Messaging.Kafka.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NovaCore.User.Infrastructure.Configurations;

/// <summary>Binds and validates every local <see cref="ISetting"/> in this assembly, then manually validates the external settings User depends on but doesn't own.</summary>
public static class ConfigurationExtensions
{
    public static IServiceCollection AddUserConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSettings(configuration, Assembly.GetExecutingAssembly());

        ValidateKafkaConfiguration(configuration);

        return services;
    }

    private static void ValidateKafkaConfiguration(IConfiguration configuration)
    {
        var kafkaOptions = configuration.GetSection(KafkaOptions.Section).Get<KafkaOptions>() ?? new KafkaOptions();
        var result = new KafkaOptionsValidator().Validate(kafkaOptions);

        if (!result.IsValid)
            throw new OptionsValidationException(KafkaOptions.Section, typeof(KafkaOptions), result.Errors.Select(e => e.ErrorMessage));
    }
}
