using System.Text.Json;
using System.Text.Json.Serialization;

namespace NovaCore.BuildingBlock.SharedKernel.Serialization;

/// <summary>
/// Centralized JSON serialization configuration for all layers and services.
/// Ensures consistent JSON handling across the entire application.
/// </summary>
public static class JsonSerializerConfiguration
{
    private static readonly JsonSerializerOptions DefaultOptions = CreateOptions();

    /// <summary>Get the default JsonSerializerOptions configured for the application</summary>
    public static JsonSerializerOptions Default => DefaultOptions;

    /// <summary>Create a new instance with default configuration</summary>
    public static JsonSerializerOptions Create() => CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}
