using System.Reflection;

using FluentValidation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NovaCore.BuildingBlock.Infrastructure.Configurations;

/// <summary>
/// Scans an assembly for <see cref="ISetting"/> classes, binds each to its configuration section,
/// and wires a fail-fast <c>ValidateOnStart()</c> check using the matching FluentValidation validator
/// found in the same assembly. Only the settings classes themselves are registered in DI - validators
/// are instantiated once to build the startup check and are never added to the container.
/// </summary>
public static class SettingsScanningExtensions
{
    private const string SettingSuffix = "Setting";
    private const BindingFlags SectionConstantFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    private static readonly MethodInfo BindSettingMethod = typeof(SettingsScanningExtensions)
        .GetMethod(nameof(BindSetting), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static IServiceCollection AddSettings(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly)
    {
        var settingTypes = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(ISetting).IsAssignableFrom(type));

        foreach (var settingType in settingTypes)
            BindSettingMethod.MakeGenericMethod(settingType).Invoke(null, [services, configuration, assembly]);

        return services;
    }

    private static void BindSetting<TSetting>(
        IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly)
        where TSetting : class, ISetting
    {
        var section = configuration.GetSection(ResolveSectionName(typeof(TSetting)));

        services
            .AddOptions<TSetting>()
            .Bind(section)
            .ValidateOnStart();

        var validatorType = assembly.GetTypes()
            .FirstOrDefault(type => type is { IsClass: true, IsAbstract: false } && typeof(IValidator<TSetting>).IsAssignableFrom(type));

        if (validatorType is not null)
        {
            var validator = (IValidator<TSetting>)Activator.CreateInstance(validatorType)!;
            services.AddSingleton<IValidateOptions<TSetting>>(new FluentValidationOptions<TSetting>(validator));
        }

        services.AddSingleton(provider => provider.GetRequiredService<IOptions<TSetting>>().Value);
    }

    /// <summary>Uses the class's own <c>const string Section</c> when declared (matches existing options like <c>KafkaOptions.Section</c>); otherwise derives it by stripping the "Setting" suffix, e.g. <c>AuthJwtSetting</c> -> "AuthJwt".</summary>
    private static string ResolveSectionName(Type settingType)
    {
        var sectionConstant = settingType.GetField("Section", SectionConstantFlags);
        if (sectionConstant is { IsLiteral: true } && sectionConstant.GetRawConstantValue() is string constantValue)
            return constantValue;

        return settingType.Name.EndsWith(SettingSuffix, StringComparison.Ordinal)
            ? settingType.Name[..^SettingSuffix.Length]
            : settingType.Name;
    }
}
