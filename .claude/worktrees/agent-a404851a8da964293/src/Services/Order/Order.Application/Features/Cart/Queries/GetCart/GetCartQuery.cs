using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Application.Features.Cart.Queries.GetCart;

public sealed record GetCartQuery : IQuery<CartResponse>;
