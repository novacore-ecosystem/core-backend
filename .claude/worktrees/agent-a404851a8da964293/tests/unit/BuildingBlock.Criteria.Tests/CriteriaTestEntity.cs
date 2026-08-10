namespace NovaCore.BuildingBlock.Criteria.Tests;

public sealed class CriteriaTestEntity
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public CriteriaTestStatus Status { get; set; }
    public string PhoneSearch { get; set; } = string.Empty;
    public string PhoneReverse { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public enum CriteriaTestStatus
{
    Active,
    Inactive,
}
