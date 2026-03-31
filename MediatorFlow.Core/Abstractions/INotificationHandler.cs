using MediatorFlow.Core.Contracts;

namespace MediatorFlow.Core.Abstractions;

public interface INotificationHandler<TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken = default);
}