using MediatorFlow.Core.Contracts;

namespace MediatorFlow.Core.Abstractions;

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}