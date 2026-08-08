using NovaCore.Promotion.Application.Abstractions.Persistence.ProductSets;
using NovaCore.Promotion.Persistence.Contexts.ProductSets.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.ProductSets.Write;

public sealed class ProductSetWriteService(IProductSetRepository productSetRepo) : IProductSetWriteService
{
}
