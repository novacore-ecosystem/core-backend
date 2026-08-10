using System.Text.Json;

using NovaCore.BuildingBlock.Criteria.Enums;

namespace NovaCore.BuildingBlock.Criteria.Requests;

public sealed record CriteriaFilter(string Field, CriteriaOperator Operator, JsonElement? Value);
