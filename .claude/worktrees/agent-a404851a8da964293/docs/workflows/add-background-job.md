# Workflow: Add Background Job

**Read first:** [06-implementation-templates.md](../06-implementation-templates.md#background-job), [services/auth-service.md](../services/auth-service.md#auth-specific-building-blocks-not-shared-with-user), [services/user-service.md](../services/user-service.md#user-specific-building-blocks-not-present-in-auth) (both are Hangfire consumers via the shared `BuildingBlock.Infrastructure` bootstrap).

The Hangfire bootstrap itself — storage/server setup (`AddHangfireWithPostgres`), recurring-job discovery (`RecurringJobRegistry`), and the scheduled-job dispatcher (`ScheduledJobScheduler`) — lives in `BuildingBlock.Infrastructure/BackgroundJobs/HangfireSchedulingExtensions.cs`. Every service reuses it; nothing about Hangfire storage/dashboard wiring is service-specific anymore. Cross-service jobs (e.g. Inbox/Outbox cleanup, see [reference/inbox-outbox-runtime.md](../reference/inbox-outbox-runtime.md#cleanup)) can live directly in `BuildingBlock.Infrastructure` and be opted into per service through their own registration call — they don't need a marker type from `AddHangfireScheduling`.

## Steps

1. Implement `IRecurringJob` (`BuildingBlock.Application.Abstractions.Jobs`) in `{Service}.Infrastructure/BackgroundJobs/Jobs/{JobName}/{JobName}Service.cs` for a service-specific job, or in `BuildingBlock.Infrastructure/BackgroundJobs/{Area}/` if it's meant to be shared across services (like the cleanup jobs).
2. Set `JobId` (unique, `{service}-{jobname}` convention), `CronExpression`, `Queue` (use `JobQueue` constants from `BuildingBlock.SharedKernel`, default `JobQueue.DEFAULT` unless there's a reason for a dedicated queue), `IsInit` (true only if it should also run once immediately at startup, not just on its cron schedule).
3. If the job needs config (thresholds, batch sizes), add an options class bound via `services.Configure<{JobName}Options>(configuration.GetSection(...))`, injected via `IOptions<T>` — see `RefreshTokenSyncService`/`RefreshTokenJobOptions` (service-specific) or `OutboxCleanupJob`/`OutboxCleanupOptions` (shared) for the pattern.
4. If the job does multi-step writes, wrap them in `IUnitOfWork.ExecuteTransactionAsync(...)` — see [04-coding-rules.md#transaction-management](../04-coding-rules.md#transaction-management).
5. This service must already call `AddBackgroundJobs(configuration)` (which wires `AddHangfireScheduling(configuration, typeof(BackgroundJobsExtensions))`), and `ApplicationPipeline.cs` must call `UseBackgroundJobsDashboard()`/`UseBackgroundJobsScheduling()`. If this is the *first* background job in a service, add its own thin `{Service}.Infrastructure/BackgroundJobs/BackgroundJobsExtensions.cs` wrapper — copy `User.Infrastructure/BackgroundJobs/BackgroundJobsExtensions.cs` (the minimal case: no service-specific jobs, just the shared cleanup jobs) rather than reimplementing the Hangfire bootstrap. A new service also needs its own `ConnectionStrings:Hangfire` (own Postgres database, e.g. `{service}_hangfire_db` — see `scripts/postgres/init.sql`) and its own Hangfire package refs are **not** required since `Hangfire.Core`/`Hangfire.AspNetCore`/`Hangfire.PostgreSql` are referenced once in `BuildingBlock.Infrastructure` and flow through transitively.
6. For a job meant to run in every service (like cleanup), add a separate opt-in extension (e.g. `AddInboxOutboxCleanupJobs(configuration)`) that each service calls independently of `AddHangfireScheduling` — see `BuildingBlock.Infrastructure/BackgroundJobs/Cleanup/CleanupJobsExtensions.cs`.

## Checklist

- [ ] `JobId` is unique across the service
- [ ] Registered automatically via `AddScopedByInterfaceAndConcrete<IRecurringJob>` — not manually added to Hangfire's registry
- [ ] Multi-step writes wrapped in an explicit transaction with rollback on failure
- [ ] Retry/backoff considered for transient failures (see `RefreshTokenSyncService` for a deadlock-retry example) if the job touches a contended table
