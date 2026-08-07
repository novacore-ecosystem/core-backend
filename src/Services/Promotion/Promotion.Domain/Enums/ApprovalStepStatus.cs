namespace NovaCore.Promotion.Domain.Enums;

public enum ApprovalStepStatus : byte
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Skipped = 3,
}
