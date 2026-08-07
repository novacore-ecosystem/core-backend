namespace NovaCore.Promotion.Domain.Enums;

public enum CampaignStatus : byte
{
    Draft = 0,
    Scheduled = 1,
    Active = 2,
    Paused = 3,
    Completed = 4,
    Cancelled = 5,
}
