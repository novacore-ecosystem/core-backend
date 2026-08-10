namespace NovaCore.BuildingBlock.Domain.Metadata;

/// <summary>
/// Declares that a property on a <see cref="MetadataBase"/> derivative is backed by the
/// internal dictionary. The dictionary key defaults to the property name (via
/// [CallerMemberName] in <see cref="MetadataBase.Get{T}"/>/<see cref="MetadataBase.Set{T}"/>)
/// unless <paramref name="key"/> is supplied.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class MetadataAttribute(string? key = null) : Attribute
{
    public string? Key { get; } = key;

    /// <summary>When true, <see cref="MetadataBase.Set{T}"/> rejects a null value and <see cref="MetadataBase.Validate"/> requires a value to be present.</summary>
    public bool Required { get; init; }

    /// <summary>Value returned by <see cref="MetadataBase.Get{T}"/> when the key has never been set.</summary>
    public object? DefaultValue { get; init; }
}
