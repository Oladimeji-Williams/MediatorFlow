using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Contracts;
using System.Linq;

namespace MediatorFlow.Core.Behaviors;

public interface IValidator<in TRequest>
{
    Task<IEnumerable<string>> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
        var validators = _validators as IValidator<TRequest>[] ?? _validators.ToArray();

        if (validators.Length > 0)
        {
            var errors = new List<string>();

            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(request, cancellationToken);
                if (result != null)
                    errors.AddRange(result);
            }

            if (errors.Count > 0)
                throw new InvalidOperationException($"Validation failed: {string.Join("; ", errors)}");
        }

        return await next();
    }
}

