using Mapster;

namespace NovaCore.Promotion.Application.Features.Coupons.Mapping;

/// <summary>
/// Value-Object-to-string conversion rules for Coupon's Entity -&gt; Response mapping (see
/// GetCouponHandler/ListCouponsHandler). Auto-discovered by Mapster's assembly scan
/// (TypeAdapterConfig.GlobalSettings.Scan in DependencyInjection.cs) - no manual registration
/// needed at each call site. Same pattern Product.Application/Common/Mapping/MapsterConfig.cs uses.
/// </summary>
public sealed class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<EntityCode, string>().MapWith(src => src.Value);
        config.NewConfig<LanguageCode, string>().MapWith(src => src.Value);
    }
}
