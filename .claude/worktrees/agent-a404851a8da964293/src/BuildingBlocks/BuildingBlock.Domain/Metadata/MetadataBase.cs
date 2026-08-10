using System.Runtime.CompilerServices;
using System.Text.Json;

using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Serialization;

namespace NovaCore.BuildingBlock.Domain.Metadata;

/// <summary>
/// Base class for strongly-typed, extensible metadata bags backed by a single
/// Dictionary&lt;string, object?&gt;. Derived classes declare ordinary properties wrapping
/// <see cref="Get{T}"/>/<see cref="Set{T}"/> - the dictionary key defaults to the property
/// name via [CallerMemberName] unless [Metadata("key")] overrides it. No derived class ever
/// touches the dictionary directly, and no backing fields are duplicated.
/// </summary>
/// <example>
/// <code>
/// public sealed class ProductMetadata : MetadataBase
/// {
///     [Metadata]
///     public string? Badge { get => Get&lt;string&gt;(); set => Set(value); }
/// }
/// </code>
/// </example>
public abstract class MetadataBase
{
    private readonly Dictionary<string, object?> _values;

    protected MetadataBase()
    {
        _values = [];
    }

    protected MetadataBase(IDictionary<string, object?> values)
    {
        _values = new Dictionary<string, object?>(values);
    }

    protected T? Get<T>([CallerMemberName] string propertyName = "")
    {
        var descriptor = MetadataPropertyMap.Resolve(GetType(), propertyName);

        if (_values.TryGetValue(descriptor.Key, out var raw) && raw is not null)
            return Coerce<T>(raw);

        return descriptor.DefaultValue is not null ? Coerce<T>(descriptor.DefaultValue) : default;
    }

    protected void Set<T>(T? value, [CallerMemberName] string propertyName = "")
    {
        var descriptor = MetadataPropertyMap.Resolve(GetType(), propertyName);

        if (descriptor.Required && value is null)
            throw ExceptionFactory.RequiredField($"Metadata field '{descriptor.Key}' is required and cannot be null.");

        _values[descriptor.Key] = value;
    }

    /// <summary>Throws if any [Metadata(Required = true)] property has never been set.</summary>
    public void Validate()
    {
        foreach (var descriptor in MetadataPropertyMap.GetAll(GetType()))
        {
            if (descriptor.Required && !_values.ContainsKey(descriptor.Key))
                throw ExceptionFactory.RequiredField($"Metadata field '{descriptor.Key}' is required.");
        }
    }

    public IReadOnlyDictionary<string, object?> ToDictionary() => _values;

    public string ToJson() => JsonSerializer.Serialize(_values, JsonSerializerConfiguration.Default);

    /// <summary>Builds an instance from a raw dictionary, e.g. one loaded from a JSONB column.</summary>
    public static TMetadata Create<TMetadata>(IDictionary<string, object?>? source = null)
        where TMetadata : MetadataBase, new()
    {
        var metadata = new TMetadata();
        if (source is not null)
        {
            foreach (var (key, value) in source)
                metadata._values[key] = value;
        }

        return metadata;
    }

    public static TMetadata FromJson<TMetadata>(string? json)
        where TMetadata : MetadataBase, new()
    {
        if (string.IsNullOrWhiteSpace(json))
            return new TMetadata();

        var values = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonSerializerConfiguration.Default);
        return Create<TMetadata>(values);
    }

    private static T? Coerce<T>(object raw)
    {
        if (raw is T typed)
            return typed;

        // Dictionary<string, object?> deserialized from JSON boxes primitives as JsonElement.
        if (raw is JsonElement element)
            return element.Deserialize<T>(JsonSerializerConfiguration.Default);

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (targetType.IsEnum && raw is string enumText)
            return (T)Enum.Parse(targetType, enumText, ignoreCase: true);

        return (T)Convert.ChangeType(raw, targetType);
    }
}
