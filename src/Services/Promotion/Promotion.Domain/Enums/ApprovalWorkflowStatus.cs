namespace NovaCore.Promotion.Domain.Enums;

public enum ApprovalWorkflowStatus : byte
{
    Draft = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
}
