namespace NovaCore.Payment.Domain.Enums;

public enum InvoiceStatus : byte
{
    Draft = 1,
    Issued = 2,
    Paid = 3,
    PartiallyPaid = 4,
    Void = 5,
    Overdue = 6,
}
