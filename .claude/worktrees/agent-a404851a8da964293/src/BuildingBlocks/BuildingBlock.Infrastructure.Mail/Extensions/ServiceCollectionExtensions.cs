using MimeKit;

using Microsoft.Extensions.DependencyInjection;

using NovaCore.BuildingBlock.Infrastructure.Mail.Abstractions;
using NovaCore.BuildingBlock.Infrastructure.Mail.Internal;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureMail(
        this IServiceCollection services,
        MailOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(options);

        services.AddSingleton(options);
        services.AddSingleton<IEmailSender, MailKitEmailSender>();

        return services;
    }

    private static void ValidateOptions(MailOptions options)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(options.Host))
            errors.Add($"{nameof(MailOptions.Host)} is required.");

        if (options.Port is <= 0 or > 65535)
            errors.Add($"{nameof(MailOptions.Port)} must be between 1 and 65535.");

        if (string.IsNullOrWhiteSpace(options.SenderEmail) || !MailboxAddress.TryParse(options.SenderEmail, out _))
            errors.Add($"{nameof(MailOptions.SenderEmail)} must be a valid email address.");

        if (errors.Count > 0)
            throw new ArgumentException($"Invalid {nameof(MailOptions)}: {string.Join(" ", errors)}", nameof(options));
    }
}
