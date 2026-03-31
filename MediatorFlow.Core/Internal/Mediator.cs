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

        return await handler.Handle((dynamic)request, cancellationToken);
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