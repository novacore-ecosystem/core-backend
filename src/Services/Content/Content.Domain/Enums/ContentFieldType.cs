namespace NovaCore.Content.Domain.Enums;

public enum ContentFieldType : byte
{
    Text = 1,
    LongText = 2,
    RichText = 3,
    Number = 4,
    Decimal = 5,
    Boolean = 6,
    Date = 7,
    DateTime = 8,
    Url = 9,
    Reference = 10,
    MultiReference = 11,
    Asset = 12,
    MultiAsset = 13,
    Json = 14,
}
