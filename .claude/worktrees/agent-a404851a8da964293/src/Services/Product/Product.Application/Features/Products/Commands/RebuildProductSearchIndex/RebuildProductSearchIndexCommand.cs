namespace NovaCore.Product.Application.Features.Products.Commands.RebuildProductSearchIndex;

public sealed record RebuildProductSearchIndexCommand : ICommand<RebuildProductSearchIndexResponse>;

public sealed record RebuildProductSearchIndexResponse(int ProductsIndexed);
