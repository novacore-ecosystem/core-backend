using NovaCore.Auth.Domain.Entities.Tenants;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

public interface ITenantWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - no caller-owned transaction exists yet.</summary>
    Task CreateAsync(Tenant tenant, CancellationToken ct = default);

    /// <summary>Load-mutate-save via the domain's own behavior methods (Rename, UpdateBranding,
    /// Deactivate, Delete, SetLocale, ...) - callers never construct EF updates directly.</summary>
    Task UpdateAsync(Guid id, Action<Tenant> update, CancellationToken ct = default);

    /// <summary>Same as UpdateAsync, but eager-loads Locales first - required whenever `update`
    /// touches SetLocale/RemoveLocale, since Tenant.SetLocale reads the in-memory Locales
    /// collection to decide insert-vs-update and an unloaded collection would look empty.</summary>
    Task UpdateWithLocalesAsync(Guid id, Action<Tenant> update, CancellationToken ct = default);

    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
