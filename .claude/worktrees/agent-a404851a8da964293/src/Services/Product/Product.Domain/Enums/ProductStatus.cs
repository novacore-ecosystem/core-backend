namespace NovaCore.Product.Domain.Enums;

public enum ProductStatus : byte
{
    Draft = 1,
    PendingApproval = 2,
    Published = 3,
    Hidden = 4,
    Archived = 5,
}
