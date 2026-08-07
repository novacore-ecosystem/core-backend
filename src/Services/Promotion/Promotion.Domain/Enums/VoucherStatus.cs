namespace NovaCore.Promotion.Domain.Enums;

public enum VoucherStatus : byte
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Issued = 3,
    Active = 4,
    Reserved = 5,
    Redeemed = 6,
    Expired = 7,
    Cancelled = 8,
    Archived = 9,
}
