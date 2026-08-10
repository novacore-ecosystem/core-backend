using NovaCore.BuildingBlock.Application.Exceptions;
using FluentValidation;
using MediatR;

namespace NovaCore.BuildingBlock.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct = default)
    {
        if (!validators.Any())
            return await next(ct);

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .Where(r => r.Errors.Count != 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count != 0)
        {
            var validationErrors = failures
                .Select(f => new ValidationError(f.PropertyName, f.ErrorMessage))
                .ToList();
            throw new Exceptions.ValidationException(validationErrors);
        }

        return await next(ct);
    }
}
