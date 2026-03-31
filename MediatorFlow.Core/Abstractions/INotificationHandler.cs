using MediatorFlow.Core.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace MediatorFlow.Core.Abstractions;

public interface INotificationHandler<TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken = default);
}