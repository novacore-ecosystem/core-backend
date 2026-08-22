namespace NovaCore.Content.Domain.Enums;

public enum PublicationStatus : byte
{
    Unpublished = 1,
    Scheduled = 2,
    Published = 3,
    Expired = 4,
    Cancelled = 5,
}
