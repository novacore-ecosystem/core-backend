namespace NovaCore.Promotion.Domain.Enums;

// TODO (structural placeholder): exact taxonomy not yet specified by the architect's design -
// values below are a conventional promotion-engine set, confirm/replace when the design is available.
public enum CampaignType : byte
{
    Seasonal = 0,
    FlashSale = 1,
    Loyalty = 2,
    Acquisition = 3,
    Retention = 4,
    Clearance = 5,
    Custom = 99,
}
