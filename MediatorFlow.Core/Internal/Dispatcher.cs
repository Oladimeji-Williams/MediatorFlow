using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Contracts;
using System.Reflection;

namespace MediatorFlow.Core.Internal;

internal class Dispatcher : IDispatcher
{
    public async Task<object?> Dispatch(object request, IServiceProvider provider, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();

        // Find the handler type
        var handlerInterface = typeof(IRequestHandler<,>).MakeGenericType(requestType, requestType.GetGenericArguments()[0]);
        var handler = provider.GetService(handlerInterface);
        if (handler == null)
            throw new InvalidOperationException($"No handler found for {requestType.Name}");

        // Get behaviors
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, requestType.GetGenericArguments()[0]);
        var behaviors = (IEnumerable<object>)provider.GetService(typeof(IEnumerable<>).MakeGenericType(behaviorType)) ?? Enumerable.Empty<object>();

        // Create the pipeline
        var method = handlerInterface.GetMethod("Handle")!;
        Func<Task<object>> next = () => (Task<object>)method.Invoke(handler, new[] { request, cancellationToken })!;

        foreach (var behavior in behaviors.Reverse())
        {
            var current = next;
            next = () => (Task<object>)behavior.GetType().GetMethod("Handle")!.Invoke(behavior, new[] { request, cancellationToken, current })!;
        }

        return await next();
    }
}