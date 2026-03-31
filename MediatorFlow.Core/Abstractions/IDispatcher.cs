using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatorFlow.Core.Abstractions;

public interface IDispatcher
{
    Task<object?> Dispatch(object request, IServiceProvider provider, CancellationToken cancellationToken);
}