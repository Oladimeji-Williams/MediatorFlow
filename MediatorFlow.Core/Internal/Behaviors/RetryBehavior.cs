using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Contracts;
using Microsoft.Extensions.Logging;

namespace MediatorFlow.Core.Internal.Behaviors;

internal class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;
    private readonly int _maxRetries;

    public RetryBehavior(ILogger<RetryBehavior<TRequest, TResponse>> logger, int maxRetries = 3)
    {
        _logger = logger;
        _maxRetries = maxRetries;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        int attempt = 0;
        while (true)
        {
            try
            {
                attempt++;
                return await next();
            }
            catch (Exception ex) when (attempt < _maxRetries)
            {
                _logger.LogWarning(ex, "Retry {Attempt} for {RequestType}", attempt, typeof(TRequest).Name);
                await Task.Delay(100 * attempt, cancellationToken); // exponential backoff
            }
        }
    }
}