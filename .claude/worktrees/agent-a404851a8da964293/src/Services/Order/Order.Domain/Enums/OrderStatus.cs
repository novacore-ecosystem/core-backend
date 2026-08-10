namespace NovaCore.Order.Domain.Enums;

public enum OrderStatus : short
{
    Pending = 1,
    Confirmed = 2,
    Processing = 3,
    Completed = 4,
    Cancelled = 5,
}
