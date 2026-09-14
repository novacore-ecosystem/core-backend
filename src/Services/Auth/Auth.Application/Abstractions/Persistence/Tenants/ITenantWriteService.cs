using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

public interface ITenantWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - no caller-owned transaction exists yet.</summary>
    Task CreateAsync(Tenant tenant, CancellationToken ct = default);

    /// <summary>Load-mutate-save via the domain's own behavior methods (Rename, UpdateBranding,
    /// Deactivate, Delete, SetTranslation, ...) - callers never construct EF updates directly.</summary>
    Task UpdateAsync(Guid id, Action<Tenant> update, CancellationToken ct = default);

    /// <summary>Same as UpdateAsync, but eager-loads Translations first - required whenever
    /// `update` touches SetTranslation/RemoveTranslation, since Tenant.SetTranslation reads the
    /// in-memory Translations collection to decide insert-vs-update and an unloaded collection
    /// would look empty.</summary>
    Task UpdateWithTranslationsAsync(Guid id, Action<Tenant> update, CancellationToken ct = default);

    Task<Tenant> UpsertTranslationAsync(
        Guid id,
        LanguageCode? language,
        string? configurationJson = null,
        string? dictionaryJson = null,
        CancellationToken ct = default);

    Task DisableAsync(Guid id, CancellationToken ct = default);

    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
