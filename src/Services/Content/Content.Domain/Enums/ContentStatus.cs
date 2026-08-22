namespace NovaCore.Content.Domain.Enums;

public enum ContentStatus : byte
{
    Draft = 1,
    InReview = 2,
    Approved = 3,
    Scheduled = 4,
    Published = 5,
    Unpublished = 6,
    Archived = 7,
    Rejected = 8,
}
