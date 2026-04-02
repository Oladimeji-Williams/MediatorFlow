using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatorFlow.Core.Behaviors;

internal class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;
    private readonly int _maxRetries;
    private readonly int _delayMs;

    public RetryBehavior(ILogger<RetryBehavior<TRequest, TResponse>> logger, int maxRetries = 3, int delayMs = 100)
    {
        _logger = logger;
        _maxRetries = maxRetries;
        _delayMs = delayMs;
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
                return await next();
            }
            catch (Exception ex) when (attempt < _maxRetries && !(ex is OperationCanceledException))
            {
                attempt++;
                _logger.LogWarning(ex, "Retry {Attempt} for {RequestType}", attempt, typeof(TRequest).Name);
                await Task.Delay(_delayMs * attempt, cancellationToken);
            }
        }
    }
}
