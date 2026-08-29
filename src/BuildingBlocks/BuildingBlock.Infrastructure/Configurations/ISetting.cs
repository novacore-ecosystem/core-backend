namespace NovaCore.BuildingBlock.Infrastructure.Configurations;

/// <summary>Marker for a settings class picked up by <see cref="SettingsScanningExtensions.AddSettings"/>. Drop the interface to pull a section out of the fail-fast startup scan without touching DI code.</summary>
public interface ISetting;
