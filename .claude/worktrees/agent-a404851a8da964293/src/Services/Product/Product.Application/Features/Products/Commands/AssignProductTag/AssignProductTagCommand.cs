namespace NovaCore.Product.Application.Features.Products.Commands.AssignProductTag;

public sealed record AssignProductTagCommand(Guid ProductId, Guid TagId) : ICommand<AssignProductTagResponse>;

public sealed record AssignProductTagResponse;
