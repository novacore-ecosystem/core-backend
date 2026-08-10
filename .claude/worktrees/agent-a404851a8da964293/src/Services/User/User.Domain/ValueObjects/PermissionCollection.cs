using System.Text.RegularExpressions;

namespace NovaCore.User.Domain.ValueObjects;

/// <summary>
/// Immutable set of permission strings (e.g. "product.product.read", "inventory.stock.adjust").
/// Stored internally in canonical (deduplicated, sorted) order so ValueObject's sequence-based
/// equality behaves as set equality regardless of the order permissions were added in. Add/Remove
/// return a new instance rather than mutating in place, per Value Object immutability rules.
/// </summary>
public sealed partial class PermissionCollection : ValueObject
{
    private static readonly PermissionCollection EmptyInstance = new([]);

    private readonly IReadOnlyList<string> _values;

    public IReadOnlyCollection<string> Values => _values;

    private PermissionCollection(IReadOnlyList<string> values)
    {
        _values = values;
    }

    public static PermissionCollection Empty => EmptyInstance;

    public static PermissionCollection Create(IEnumerable<string>? permissions = null)
        => new(Normalize(permissions ?? []));

    /// <summary>Combines multiple roles' permission sets into one, deduplicated - the merge
    /// operation behind a User's Permission Snapshot.</summary>
    public static PermissionCollection Merge(IEnumerable<PermissionCollection> collections)
        => Create(collections.SelectMany(c => c.Values));

    public PermissionCollection Add(string permission)
    {
        var normalizedPermission = NormalizeOne(permission);
        return _values.Contains(normalizedPermission)
            ? this
            : new PermissionCollection(Normalize(_values.Append(normalizedPermission)));
    }

    public PermissionCollection Remove(string permission)
    {
        var normalizedPermission = NormalizeOne(permission);
        return !_values.Contains(normalizedPermission)
            ? this
            : new PermissionCollection(_values.Where(v => v != normalizedPermission).ToArray());
    }

    public bool Contains(string permission) => _values.Contains(NormalizeOne(permission));

    private static IReadOnlyList<string> Normalize(IEnumerable<string> permissions)
        => permissions
            .Select(NormalizeOne)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToArray();

    private static string NormalizeOne(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw ExceptionFactory.RequiredField("Permission cannot be empty.");

        var normalized = permission.Trim().ToLowerInvariant();

        if (!PermissionFormat().IsMatch(normalized))
            throw ExceptionFactory.InvalidFormat(
                "Permission must be lowercase dot-separated segments, e.g. \"product.product.read\".");

        return normalized;
    }

    [GeneratedRegex("^[a-z][a-z0-9_]*(\\.[a-z][a-z0-9_]*)+$")]
    private static partial Regex PermissionFormat();

    public override IEnumerable<object> GetEqualityComponents() => _values;

    public override string ToString() => string.Join(", ", _values);
}
