using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Contracts;

namespace MediatorFlow.Core.Internal;

internal class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDispatcher _dispatcher;

    public Mediator(IServiceProvider serviceProvider, IDispatcher dispatcher)
    {
        _serviceProvider = serviceProvider;
        _dispatcher = dispatcher;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var result = await _dispatcher.Dispatch(
            request,
            _serviceProvider,
            cancellationToken);
        if (result is not TResponse typed)
            throw new InvalidOperationException($"Invalid response type for {request.GetType().Name}");

        return typed;
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