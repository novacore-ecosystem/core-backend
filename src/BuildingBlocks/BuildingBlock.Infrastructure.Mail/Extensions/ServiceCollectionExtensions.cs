using MimeKit;

using Microsoft.Extensions.DependencyInjection;

using NovaCore.BuildingBlock.Infrastructure.Mail.Abstractions;
using NovaCore.BuildingBlock.Infrastructure.Mail.Internal;

using Resend;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>Single integration point for this building block - registers Resend as the <see cref="IEmailSender"/> implementation. The Resend SDK stays internal here; callers only ever see <see cref="IEmailSender"/>.</summary>
    public static IServiceCollection AddInfrastructureMail(
        this IServiceCollection services,
        ResendMailOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            errors.Add($"{nameof(ResendMailOptions.ApiKey)} is required.");

        if (string.IsNullOrWhiteSpace(options.SenderEmail) || !MailboxAddress.TryParse(options.SenderEmail, out _))
            errors.Add($"{nameof(ResendMailOptions.SenderEmail)} must be a valid email address.");

        if (errors.Count > 0)
            throw new ArgumentException($"Invalid {nameof(ResendMailOptions)}: {string.Join(" ", errors)}", nameof(options));

        services.AddSingleton(options);
        services.AddResend(resendOptions => resendOptions.ApiToken = options.ApiKey);
        services.AddSingleton<IEmailSender, ResendEmailSender>();

        return services;
    }
}
