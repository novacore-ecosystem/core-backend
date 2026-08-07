namespace NovaCore.Promotion.Domain.Enums;

public enum ReservationStatus : byte
{
    Reserved = 0,
    Released = 1,
    Expired = 2,
    Consumed = 3,
}
