namespace NovaCore.Product.Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid ProductId) : ICommand<DeleteProductResponse>;

public sealed record DeleteProductResponse;
