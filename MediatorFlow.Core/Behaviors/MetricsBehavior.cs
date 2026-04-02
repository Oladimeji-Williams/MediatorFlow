using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MediatorFlow.Core.Behaviors;
public class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<MetricsBehavior<TRequest, TResponse>> _logger;

    public MetricsBehavior(ILogger<MetricsBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();
_logger.LogInformation(
    "Request {RequestType} executed in {ElapsedMs}ms",
    typeof(TRequest).Name,
    stopwatch.ElapsedMilliseconds);
        Console.WriteLine($"Request {typeof(TRequest).Name} executed in {stopwatch.ElapsedMilliseconds}ms");
        return response;
    }
}

