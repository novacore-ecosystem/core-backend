using System.Collections.Concurrent;
using System.Reflection;

namespace NovaCore.BuildingBlock.Domain.Metadata;

/// <summary>
/// Reflection cache mapping a <see cref="MetadataBase"/> derivative's property names to their
/// [Metadata] descriptor, built once per type. Internal - MetadataBase is the only consumer.
/// </summary>
internal static class MetadataPropertyMap
{
    internal readonly record struct Descriptor(string Key, bool Required, object? DefaultValue);

    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, Descriptor>> Cache = new();

    internal static Descriptor Resolve(Type type, string propertyName)
    {
        var map = Cache.GetOrAdd(type, BuildMap);

        if (!map.TryGetValue(propertyName, out var descriptor))
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}' on '{type.Name}' must be decorated with [Metadata] to be used with MetadataBase.Get/Set.");
        }

        return descriptor;
    }

    internal static IEnumerable<Descriptor> GetAll(Type type) => Cache.GetOrAdd(type, BuildMap).Values;

    private static IReadOnlyDictionary<string, Descriptor> BuildMap(Type type)
    {
        var map = new Dictionary<string, Descriptor>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var attribute = property.GetCustomAttribute<MetadataAttribute>();
            if (attribute is null)
                continue;

            map[property.Name] = new Descriptor(attribute.Key ?? property.Name, attribute.Required, attribute.DefaultValue);
        }

        return map;
    }
}
