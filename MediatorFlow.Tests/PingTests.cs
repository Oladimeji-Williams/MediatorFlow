using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// Sample request
public record PingRequest() : IRequest<string>;

// Sample handler
public class PingHandler : IRequestHandler<PingRequest, string>
{
    public Task<string> Handle(PingRequest request, CancellationToken cancellationToken)
        => Task.FromResult("Pong");
}

// Test pipeline behavior
public class LoggingBehaviorForTest<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public bool Entered { get; private set; } = false;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        Entered = true;
        return await next();
    }
}

// Unit test class
public class MediatorPipelineTests
{
    [Fact]
    public async Task Send_Should_Invoke_Handler_And_Pipeline()
    {
        var services = new ServiceCollection();

        // Register logging, pipeline, handler, mediator
        services.AddSingleton<ILogger<LoggingBehaviorForTest<PingRequest, string>>, NullLogger<LoggingBehaviorForTest<PingRequest, string>>>();
        services.AddSingleton<IPipelineBehavior<PingRequest, string>, LoggingBehaviorForTest<PingRequest, string>>();
        services.AddSingleton<IRequestHandler<PingRequest, string>, PingHandler>();
        services.AddSingleton<IMediator, MediatorFlow.Core.Internal.Mediator>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var behavior = provider.GetRequiredService<IPipelineBehavior<PingRequest, string>>() as LoggingBehaviorForTest<PingRequest, string>;

        var result = await mediator.Send(new PingRequest());

        Assert.Equal("Pong", result);
        Assert.True(behavior!.Entered);
    }
}