namespace NovaCore.Promotion.Domain.Enums;

public enum CouponStatus : byte
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Scheduled = 3,
    Active = 4,
    Paused = 5,
    Expired = 6,
    Cancelled = 7,
    Archived = 8,
}
