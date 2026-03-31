using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Contracts;

namespace MediatorFlow.Core.Internal.Behaviors;

internal interface IValidator<in TRequest>
{
    Task<IEnumerable<string>> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}

internal class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        if (_validators != null && _validators.Any())
        {
            var errors = new List<string>();
            foreach (var validator in _validators)
            {
                var result = await validator.ValidateAsync(request, cancellationToken);
                if (result != null)
                    errors.AddRange(result);
            }

            if (errors.Any())
                throw new InvalidOperationException($"Validation failed: {string.Join("; ", errors)}");
        }

        return await next();
    }
}