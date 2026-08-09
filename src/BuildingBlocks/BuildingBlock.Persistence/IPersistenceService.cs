namespace NovaCore.BuildingBlock.Persistence;

/// <summary>
/// Marker for a Persistence Service - an Application-facing abstraction over a meaningful group of
/// data-access operations (one or more Repositories, cache, or both), not a single Repository-
/// method decorator. Implementing this alongside a service's own application-facing interface(s)
/// (e.g. `sealed class RolePersistenceService : IRolePersistenceService, IPersistenceService`) is
/// what makes it eligible for automatic Scrutor discovery - see AddScopedByInterface
/// (BuildingBlock.Application/Extensions/ServiceScanningExtensions.cs), the same generic scanning
/// helper `IRepository&lt;&gt;` already uses, called with typeof(IPersistenceService) instead. No
/// manual services.AddScoped&lt;IXyz, Xyz&gt;() is needed once a class implements this.
///
/// Carries no members - its only purpose is classification. A class should implement it only when
/// it genuinely represents a meaningful persistence concern; do not implement it on a Repository,
/// cache helper, mapper, or other class merely to get automatic registration (see
/// docs/services/auth-service.md's Persistence refactor phase for the reasoning).
/// </summary>
public interface IPersistenceService
{
}
