using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Contracts;

namespace MediatorFlow.Core.Internal;

internal class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        // Resolve the handler
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        dynamic handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
            throw new InvalidOperationException($"No handler registered for {request.GetType().Name}");

        // Resolve pipeline behaviors
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var behaviors = ((IEnumerable<object>?)_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(behaviorType))) ?? Enumerable.Empty<object>();

        // Build pipeline delegate chain
        RequestHandlerDelegate<TResponse> handlerDelegate = () => handler.Handle((dynamic)request, cancellationToken);

        foreach (dynamic behavior in behaviors.Reverse()) // reverse to wrap outermost first
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.Handle((dynamic)request, cancellationToken, next);
        }

        return await handlerDelegate();
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));

        // Resolve all handlers
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());
        var handlers = ((IEnumerable<object>)_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(handlerType))) ?? Enumerable.Empty<object>();

        foreach (dynamic handler in handlers)
        {
            await handler.Handle((dynamic)notification, cancellationToken);
        }
    }
}