using System.Text.Json.Serialization;

namespace NovaCore.BuildingBlock.Criteria.Requests;

[JsonConverter(typeof(LowerCaseStringEnumConverter<SortDirection>))]
public enum SortDirection
{
    Asc,
    Desc,
}
