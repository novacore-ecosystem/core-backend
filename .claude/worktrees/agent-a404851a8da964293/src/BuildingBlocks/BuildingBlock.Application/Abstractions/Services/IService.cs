namespace NovaCore.BuildingBlock.Application.Abstractions.Services;

/// <summary>
/// Marker interface for application business services.
/// Enables automatic service discovery and registration via Scrutor scanning.
/// All application-layer services (Stock validation, Workflows, etc.) should implement this.
/// </summary>
public interface IService
{
}
