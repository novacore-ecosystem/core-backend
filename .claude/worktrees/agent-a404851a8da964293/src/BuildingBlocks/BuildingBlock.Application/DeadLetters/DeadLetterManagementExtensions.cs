using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NovaCore.BuildingBlock.Application.Abstractions.Idempotency;
using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Messaging.Abstractions;

namespace NovaCore.BuildingBlock.Application.DeadLetters;

public static class DeadLetterManagementExtensions
{
    /// <summary>
    /// Registers the provider-agnostic dead-letter retry service. The provider-specific
    /// IDeadLetterQueryService (EF or Mongo) is registered separately via
    /// AddEfDeadLetterQueryService&lt;TContext&gt;/AddMongoDeadLetterQueryService&lt;TContext&gt;
    /// in the service's Persistence project, next to its existing AddOutboxAndInbox() call.
    ///
    /// IDistributedLockProvider is resolved as optional (via GetService, not GetRequiredService) -
    /// only User/Order/Product currently call AddIdempotency(); Auth has Redis but not
    /// AddIdempotency(), and Audit/Inventory/Notification have no Redis at all.
    /// DeadLetterRetryService degrades gracefully without it (see its doc comment).
    ///
    /// Callers must also add this assembly to their MediatR/Carter scans:
    ///   config.RegisterServicesFromAssembly(typeof(IDeadLetterRetryService).Assembly)
    ///   services.AddCarterModules(typeof(DependencyInjection), typeof(IDeadLetterRetryService))
    /// </summary>
    public static IServiceCollection AddDeadLetterManagement(this IServiceCollection services)
    {
        services.AddScoped<IDeadLetterRetryService>(sp => new DeadLetterRetryService(
            sp.GetRequiredService<IInboxStore>(),
            sp.GetRequiredService<IOutboxPublisher>(),
            sp.GetService<IDistributedLockProvider>(),
            sp.GetRequiredService<ICurrentUserService>(),
            sp.GetRequiredService<ILogger<DeadLetterRetryService>>()));

        return services;
    }
}
