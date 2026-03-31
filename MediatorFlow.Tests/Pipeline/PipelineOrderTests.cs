using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MediatorFlow.Application.Ping;
namespace MediatorFlow.Tests.Pipeline;


// Behavior
public class OrderTrackingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly List<string> _log;

    public OrderTrackingBehavior(List<string> log)
    {
        _log = log;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        _log.Add($"Before {typeof(TRequest).Name}");

        var response = await next();

        _log.Add($"After {typeof(TRequest).Name}");

        return response;
    }
}

// Test
public class PipelineOrderTests
{
    [Fact]
    public async Task Pipeline_Should_Execute_In_Correct_Order()
    {
        var services = new ServiceCollection();

        var log = new List<string>();
        services.AddSingleton(log);
        services.AddSingleton<IDispatcher, MediatorFlow.Generated.GeneratedDispatcher>();

        // Register handler
        services.AddSingleton<IRequestHandler<PingRequest, string>, TrackingHandler>();

        // Register behaviors (twice to simulate stacking)
        services.AddSingleton<IPipelineBehavior<PingRequest, string>, OrderTrackingBehavior<PingRequest, string>>();
        services.AddSingleton<IPipelineBehavior<PingRequest, string>, OrderTrackingBehavior<PingRequest, string>>();

        // Register mediator
        services.AddSingleton<IMediator, MediatorFlow.Core.Internal.Mediator>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new PingRequest());

        Assert.Equal("Pong", result);

        Assert.Equal(5, log.Count);
        Assert.Equal("Before PingRequest", log[0]);
        Assert.Equal("Before PingRequest", log[1]);
        Assert.Equal("Handler Executed", log[2]);
        Assert.Equal("After PingRequest", log[3]);
        Assert.Equal("After PingRequest", log[4]);
    }
}