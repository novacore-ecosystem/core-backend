using MediatR;

namespace NovaCore.BuildingBlock.Application.Abstractions.CQRS;

public interface IQuery<TResponse> : IRequest<TResponse>
{
}