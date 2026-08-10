using MediatR;

namespace NovaCore.BuildingBlock.Application.Abstractions.CQRS;

public interface ICommand : IRequest
{
}

public interface ICommand<TResponse> : IRequest<TResponse>
{
}
