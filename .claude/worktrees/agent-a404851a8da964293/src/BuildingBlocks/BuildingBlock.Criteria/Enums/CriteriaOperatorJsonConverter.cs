using System.Text.Json;
using System.Text.Json.Serialization;

namespace NovaCore.BuildingBlock.Criteria.Enums;

/// <summary>Maps the short API-facing operator codes (eq/ne/gt/gte/lt/lte/c/sw/ew/in/nin/between/null/notnull) to/from <see cref="CriteriaOperator"/> so the wire contract never leaks the enum's C# member names.</summary>
public sealed class CriteriaOperatorJsonConverter : JsonConverter<CriteriaOperator>
{
    private static readonly IReadOnlyDictionary<string, CriteriaOperator> FromCode = new Dictionary<string, CriteriaOperator>(StringComparer.OrdinalIgnoreCase)
    {
        ["eq"] = CriteriaOperator.Eq,
        ["ne"] = CriteriaOperator.Ne,
        ["gt"] = CriteriaOperator.Gt,
        ["gte"] = CriteriaOperator.Gte,
        ["lt"] = CriteriaOperator.Lt,
        ["lte"] = CriteriaOperator.Lte,
        ["c"] = CriteriaOperator.Contains,
        ["sw"] = CriteriaOperator.StartsWith,
        ["ew"] = CriteriaOperator.EndsWith,
        ["in"] = CriteriaOperator.In,
        ["nin"] = CriteriaOperator.NotIn,
        ["between"] = CriteriaOperator.Between,
        ["null"] = CriteriaOperator.IsNull,
        ["notnull"] = CriteriaOperator.IsNotNull,
    };

    private static readonly IReadOnlyDictionary<CriteriaOperator, string> ToCode =
        FromCode.GroupBy(kv => kv.Value).ToDictionary(g => g.Key, g => g.First().Key);

    public override CriteriaOperator Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var code = reader.GetString();
        if (code is not null && FromCode.TryGetValue(code, out var value))
            return value;

        throw new JsonException($"Unknown criteria operator '{code}'.");
    }

    public override void Write(Utf8JsonWriter writer, CriteriaOperator value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(ToCode[value]);
    }
}
