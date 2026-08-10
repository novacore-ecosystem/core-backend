using System.Globalization;
using System.Text.Json;

using NovaCore.BuildingBlock.Criteria.Enums;

namespace NovaCore.BuildingBlock.Criteria.Building;

/// <summary>Converts the wire-level <see cref="JsonElement"/> filter value into the CLR type of the field's own property, based on the field's declared <see cref="CriteriaFieldType"/>.</summary>
internal static class CriteriaValueConverter
{
    public static object? Convert(JsonElement? value, Type targetType, CriteriaFieldType fieldType)
        => value is null ? null : ConvertElement(value.Value, targetType, fieldType);

    public static List<object?> ConvertMany(JsonElement? value, Type targetType, CriteriaFieldType fieldType)
    {
        if (value is not { ValueKind: JsonValueKind.Array } array)
            return [];

        return array.EnumerateArray().Select(e => ConvertElement(e, targetType, fieldType)).ToList();
    }

    private static object? ConvertElement(JsonElement element, Type targetType, CriteriaFieldType fieldType)
    {
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (fieldType == CriteriaFieldType.Enum && underlying.IsEnum)
            return Enum.Parse(underlying, element.GetString()!, ignoreCase: true);

        if (underlying == typeof(Guid))
            return Guid.Parse(element.GetString()!);

        if (underlying == typeof(DateTime))
            return element.GetDateTime();

        if (underlying == typeof(DateTimeOffset))
            return element.GetDateTimeOffset();

        if (underlying == typeof(bool))
            return element.GetBoolean();

        if (underlying == typeof(string))
            return element.GetString();

        return System.Convert.ChangeType(element.GetRawText(), underlying, CultureInfo.InvariantCulture);
    }
}
