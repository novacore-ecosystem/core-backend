using NovaCore.BuildingBlock.Application.Abstractions.Jobs;
using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Monitoring;
using NovaCore.BuildingBlock.Infrastructure.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;

/// <summary>
/// Opt-in registration for the Inbox/Outbox cleanup recurring jobs, plus the Inbox
/// dead-letter monitor (same opt-in surface since both only need Hangfire scheduling and
/// IOutboxStore/IInboxStore already wired up). Independent of AddHangfireScheduling's own
/// job-assembly markers - a service just needs Hangfire scheduling wired up (via
/// AddHangfireScheduling) and its IOutboxStore/IInboxStore registered (via
/// AddOutboxAndInbox) for these jobs to resolve and run.
/// </summary>
public static class CleanupJobsExtensions
{
    public static IServiceCollection AddInboxOutboxCleanupJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .Configure<OutboxCleanupOptions>(configuration.GetSection(OutboxCleanupOptions.Section))
            .Configure<InboxCleanupOptions>(configuration.GetSection(InboxCleanupOptions.Section))
            .Configure<InboxDeadLetterMonitorOptions>(configuration.GetSection(InboxDeadLetterMonitorOptions.Section))
            .AddScopedByInterfaceAndConcrete<IRecurringJob>(typeof(OutboxCleanupJob));

        return services;
    }
}
