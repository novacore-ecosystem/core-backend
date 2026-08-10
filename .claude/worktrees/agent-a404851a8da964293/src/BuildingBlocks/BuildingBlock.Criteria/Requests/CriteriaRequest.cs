namespace NovaCore.BuildingBlock.Criteria.Requests;

public sealed record CriteriaRequest
{
    public string? Keyword { get; init; }
    public IReadOnlyList<CriteriaFilter> Filters { get; init; } = [];
    public IReadOnlyList<CriteriaSort> Sorts { get; init; } = [];
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
