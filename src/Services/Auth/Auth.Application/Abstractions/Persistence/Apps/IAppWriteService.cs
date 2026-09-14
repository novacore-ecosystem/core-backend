using NovaCore.Auth.Domain.Entities.Apps;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Apps;

public interface IAppWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - no caller-owned transaction exists yet.</summary>
    Task CreateAsync(App app, CancellationToken ct = default);

    /// <summary>Load-mutate-save via the domain's own behavior methods (Rename, Activate,
    /// Deactivate, ...) - callers never construct EF updates directly.</summary>
    Task UpdateAsync(Guid id, Action<App> update, CancellationToken ct = default);

    /// <summary>Same as UpdateAsync, but eager-loads Translations first - required whenever
    /// `update` calls App.Translate, since it reads the in-memory Translations collection to
    /// decide insert-vs-update and an unloaded collection would look empty.</summary>
    Task UpdateWithTranslationsAsync(Guid id, Action<App> update, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
