namespace NovaCore.Notification.Domain.ValueObjects;

/// <summary>
/// What triggered a <see cref="Entities.NotificationDispatch"/> - an open (string, string) pair
/// rather than a closed enum, same reasoning as Audit's RootEntityType/RootEntityId: the set of
/// possible source entities/aggregates across every producing service is unbounded and evolves
/// without this service's involvement.
/// </summary>
public sealed class DispatchReference : ValueObject
{
    public string ReferenceType { get; }
    public string ReferenceId { get; }

    private DispatchReference(string referenceType, string referenceId)
    {
        ReferenceType = referenceType;
        ReferenceId = referenceId;
    }

    public static bool IsValid(string? referenceType, string? referenceId) =>
        GetValidationError(referenceType, referenceId) is null;

    public static bool TryCreate(string? referenceType, string? referenceId, out DispatchReference? reference)
    {
        if (GetValidationError(referenceType, referenceId) is not null)
        {
            reference = null;
            return false;
        }

        reference = new DispatchReference(referenceType!.Trim(), referenceId!.Trim());
        return true;
    }

    public static DispatchReference Create(string referenceType, string referenceId)
    {
        var error = GetValidationError(referenceType, referenceId);
        if (error is not null)
            throw error;

        return new DispatchReference(referenceType.Trim(), referenceId.Trim());
    }

    private static InvalidArgumentException? GetValidationError(string? referenceType, string? referenceId)
    {
        if (string.IsNullOrWhiteSpace(referenceType))
            return ExceptionFactory.RequiredField("Dispatch reference type cannot be empty.");

        if (string.IsNullOrWhiteSpace(referenceId))
            return ExceptionFactory.RequiredField("Dispatch reference id cannot be empty.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return ReferenceType;
        yield return ReferenceId;
    }
}
