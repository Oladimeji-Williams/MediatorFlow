using MediatorFlow.Core.Abstractions;
using MediatorFlow.Application.Ping;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediatorFlow.Tests.Pipeline;

public class TrackingHandler : IRequestHandler<PingRequest, string>
{
    private readonly List<string> _log;

    public TrackingHandler(List<string> log)
    {
        _log = log;
    }

    public Task<string> Handle(PingRequest request, CancellationToken cancellationToken)
    {
        _log.Add("Handler Executed");
        return Task.FromResult("Pong");
    }
}