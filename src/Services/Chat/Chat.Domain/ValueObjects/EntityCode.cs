using System.Text.RegularExpressions;

namespace NovaCore.Chat.Domain.ValueObjects;

/// <summary>
/// Shared uppercase, human-assigned identifier used by every catalog-style aggregate root's own
/// Code property (Tag, ConversationQueue, ConversationRole, ConversationPermission, Sticker) -
/// same consolidation rationale as Promotion.Domain's EntityCode (see
/// docs/promotion-service/value-objects/README.md): one shared VO reused across aggregates within
/// this Domain project, instead of a structurally-identical per-aggregate Code VO for each.
/// </summary>
public sealed partial class EntityCode : StringValueObject
{
    private const int MaxLength = 50;

    private EntityCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out EntityCode? code)
    {
        if (GetValidationError(value) is not null)
        {
            code = null;
            return false;
        }

        code = new EntityCode(Normalize(value!));
        return true;
    }

    public static EntityCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new EntityCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Code cannot exceed {MaxLength} characters.");

        if (!CodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Code must be uppercase alphanumeric with underscores/hyphens only.");

        return null;
    }

    [GeneratedRegex("^[A-Z0-9]+([_-][A-Z0-9]+)*$")]
    private static partial Regex CodeFormat();
}
