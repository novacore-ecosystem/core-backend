using System.Text.Json.Nodes;

namespace NovaCore.Auth.Application.Common;

/// <summary>
/// Deep-merges tenant locale JSON blobs (ConfigurationJson/DictionaryJson) - override wins key by
/// key, nested objects merge recursively, unrelated keys in the base are preserved. Used both to
/// compute an effective (fallback + language override) view for reads, and to apply a partial
/// update onto stored JSON without wiping out unrelated keys.
/// </summary>
internal static class JsonMergeHelper
{
    public static JsonObject Merge(string baseJson, string? overrideJson)
    {
        var baseObject = JsonNode.Parse(baseJson)?.AsObject() ?? [];

        if (string.IsNullOrWhiteSpace(overrideJson))
            return baseObject;

        var overrideObject = JsonNode.Parse(overrideJson)?.AsObject();

        return overrideObject is null ? baseObject : MergeObjects(baseObject, overrideObject);
    }

    private static JsonObject MergeObjects(JsonObject baseObject, JsonObject overrideObject)
    {
        var result = new JsonObject();

        foreach (var (key, value) in baseObject)
            result[key] = value?.DeepClone();

        foreach (var (key, value) in overrideObject)
        {
            result[key] = value is JsonObject overrideChild && result[key] is JsonObject baseChild
                ? MergeObjects(baseChild, overrideChild)
                : value?.DeepClone();
        }

        return result;
    }
}
