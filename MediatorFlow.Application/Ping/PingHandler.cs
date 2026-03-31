using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace MediatorFlow.Application.Ping;

public record PingRequest() : IRequest<string>;

public class PingHandler : IRequestHandler<PingRequest, string>
{
    public Task<string> Handle(PingRequest request, CancellationToken cancellationToken)
        => Task.FromResult("Pong");
}