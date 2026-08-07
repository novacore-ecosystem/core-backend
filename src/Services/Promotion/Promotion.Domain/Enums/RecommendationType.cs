namespace NovaCore.Promotion.Domain.Enums;

public enum RecommendationType : byte
{
    CrossSell = 0,
    UpSell = 1,
    FrequentlyBoughtTogether = 2,
    Trending = 3,
    Manual = 4,
    AI = 5,
}
