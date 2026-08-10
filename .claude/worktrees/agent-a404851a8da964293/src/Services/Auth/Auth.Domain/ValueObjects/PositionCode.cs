using System.Text.RegularExpressions;

namespace NovaCore.Auth.Domain.ValueObjects;

/// <summary>
/// Lowercase snake_case internal identifier for a Position (e.g. "office_manager").
/// Language-independent and never translated - the localized display text lives on
/// PositionTranslation.DisplayName. Mirrors RoleCode's shape.
/// </summary>
public sealed partial class PositionCode : StringValueObject
{
    private const int MaxLength = 100;

    private PositionCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out PositionCode? positionCode)
    {
        if (GetValidationError(value) is not null)
        {
            positionCode = null;
            return false;
        }

        positionCode = new PositionCode(Normalize(value!));
        return true;
    }

    public static PositionCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new PositionCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Position code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Position code cannot exceed {MaxLength} characters.");

        if (!PositionCodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Position code must be lowercase snake_case (letters, digits, underscores), e.g. \"office_manager\".");

        return null;
    }

    [GeneratedRegex("^[a-z][a-z0-9]*(_[a-z0-9]+)*$")]
    private static partial Regex PositionCodeFormat();
}
