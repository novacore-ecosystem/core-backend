using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Apps;

public interface IAppReadService
{
    /// <summary>Includes Translations - the full editing payload for the App Management screen.</summary>
    Task<App?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>The stable-identifier lookup used by Register/Login to resolve the App the
    /// frontend hardcoded - no Translations include, this is a hot request-time path.</summary>
    Task<App?> GetByCodeAsync(AppCode code, CancellationToken ct = default);

    Task<bool> ExistsByCodeAsync(AppCode code, CancellationToken ct = default);

    /// <summary>Database-level search + pagination for the App Management list screen - matches
    /// against Code/Name, case-insensitive.</summary>
    Task<(IReadOnlyList<App> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
