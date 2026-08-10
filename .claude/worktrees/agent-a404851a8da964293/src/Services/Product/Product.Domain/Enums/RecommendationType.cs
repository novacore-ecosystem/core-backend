namespace NovaCore.Product.Domain.Enums;

public enum RecommendationType : short
{
    Manual = 1,
    Related = 2,
    CrossSell = 3,
    Upsell = 4,
    Trending = 5,
    BestSeller = 6,
    FrequentlyBoughtTogether = 7,
    RecentlyViewed = 8,
    Personalized = 9,
    AIRecommended = 10,
}
