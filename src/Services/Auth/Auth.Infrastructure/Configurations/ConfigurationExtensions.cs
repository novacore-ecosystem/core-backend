using System.Reflection;

using NovaCore.Auth.Application.Configurations;
using NovaCore.Auth.Infrastructure.Configurations.Settings;
using NovaCore.Auth.Infrastructure.Configurations.Validators;

using NovaCore.BuildingBlock.Infrastructure.Configurations;
using NovaCore.BuildingBlock.Messaging.Kafka.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NovaCore.Auth.Infrastructure.Configurations;

/// <summary>Binds and validates every local <see cref="ISetting"/> in this assembly, then manually validates the external settings Auth depends on but doesn't own.</summary>
public static class ConfigurationExtensions
{
    public static IServiceCollection AddAuthConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSettings(configuration, Assembly.GetExecutingAssembly());
        services.AddSingleton(BindSessionJwtSetting(configuration));
        services.AddSingleton(BindClientSetting(configuration));

        ValidateKafkaConfiguration(configuration);
        ValidateRootConfiguration(configuration);
        ValidateClientConfiguration(configuration);

        return services;
    }

    /// <summary>
    /// SessionJwtSetting lives in Auth.Application (OnAuthenticationSucceededHandler reads it
    /// directly) so it can't join this assembly's ISetting reflection scan - bound manually here
    /// instead, the same way RootSetting is exposed as a plain instance in Auth.Persistence's
    /// AddSeeding.
    /// </summary>
    private static SessionJwtSetting BindSessionJwtSetting(IConfiguration configuration)
        => configuration.GetSection(SessionJwtSetting.Section).Get<SessionJwtSetting>() ?? new SessionJwtSetting();

    /// <summary>ClientSetting lives in Auth.Application (ForgotPasswordHandler/RegisterHandler read it directly) so it can't join this assembly's ISetting reflection scan - bound manually here, the same way SessionJwtSetting is.</summary>
    private static ClientSetting BindClientSetting(IConfiguration configuration)
        => configuration.GetSection(ClientSetting.Section).Get<ClientSetting>() ?? new ClientSetting();

    private static void ValidateKafkaConfiguration(IConfiguration configuration)
    {
        var kafkaOptions = configuration.GetSection(KafkaOptions.Section).Get<KafkaOptions>() ?? new KafkaOptions();
        var result = new KafkaOptionsValidator().Validate(kafkaOptions);

        if (!result.IsValid)
            throw new OptionsValidationException(
                KafkaOptions.Section,
                typeof(KafkaOptions),
                result.Errors.Select(e => e.ErrorMessage));
    }

    /// <summary>
    /// RootSetting lives in Auth.Application (RefreshTokenHandler reads it directly) so it can't
    /// join this assembly's ISetting reflection scan - validated manually here instead, the same
    /// way ValidateKafkaConfiguration handles KafkaOptions.
    /// </summary>
    private static void ValidateRootConfiguration(IConfiguration configuration)
    {
        var rootSetting = configuration.GetSection(RootSetting.Section).Get<RootSetting>() ?? new RootSetting();
        var result = new RootSettingValidator().Validate(rootSetting);

        if (!result.IsValid)
            throw new OptionsValidationException(
                RootSetting.Section,
                typeof(RootSetting),
                result.Errors.Select(e => e.ErrorMessage));
    }

    private static void ValidateClientConfiguration(IConfiguration configuration)
    {
        var clientSetting = configuration.GetSection(ClientSetting.Section).Get<ClientSetting>() ?? new ClientSetting();
        var result = new ClientSettingValidator().Validate(clientSetting);

        if (!result.IsValid)
            throw new OptionsValidationException(
                ClientSetting.Section,
                typeof(ClientSetting),
                result.Errors.Select(e => e.ErrorMessage));
    }
}
