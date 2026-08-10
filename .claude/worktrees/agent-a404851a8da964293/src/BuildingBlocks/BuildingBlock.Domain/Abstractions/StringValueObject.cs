namespace NovaCore.BuildingBlock.Domain.Abstractions;

/// <summary>
/// Base for single-string Value Objects (codes, slugs, identifiers) that only need
/// value equality plus a normalized underlying string. Concrete types supply their
/// own <c>Create</c> factory with format-specific validation.
/// </summary>
public abstract class StringValueObject(string value) : ValueObject
{
    public string Value { get; } = value;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
