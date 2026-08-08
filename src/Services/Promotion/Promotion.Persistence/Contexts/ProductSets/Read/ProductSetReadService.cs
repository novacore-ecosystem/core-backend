using NovaCore.Promotion.Application.Abstractions.Persistence.ProductSets;
using NovaCore.Promotion.Persistence.Contexts.ProductSets.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.ProductSets.Read;

public sealed class ProductSetReadService(IProductSetRepository productSetRepo) : IProductSetReadService
{
}
