# MediatorFlow

A modern mediator pattern library for .NET with built-in pipeline behaviors and source generator for auto-registration.

## Features

- **Mediator Pattern**: Clean separation of concerns with requests and notifications.
- **Pipeline Behaviors**: Built-in logging, metrics, retry, and validation behaviors.
- **Source Generator**: Automatic registration of handlers at compile-time.
- **Clean Architecture**: Modular design with separate core, application, and generator projects.

## Installation

Install the main package:
```
dotnet add package MediatorFlow.Core
```

For the source generator:
```
dotnet add package MediatorFlow.Generator
```

## Usage

1. Define a request:
```csharp
public record PingRequest() : IRequest<string>;
```

2. Implement a handler:
```csharp
public class PingHandler : IRequestHandler<PingRequest, string>
{
    public Task<string> Handle(PingRequest request, CancellationToken cancellationToken)
        => Task.FromResult("Pong");
}
```

3. Register in DI:
```csharp
services.AddMediatorFlow();
services.AddMediatorFlowApplication(); // For source generator
```

4. Use the mediator:
```csharp
var result = await mediator.Send(new PingRequest());
```

## Pipeline Behaviors

MediatorFlow includes several built-in behaviors:
- **Validation**: Validates requests using `IValidator<TRequest>`.
- **Retry**: Retries failed requests with configurable attempts and delay.
- **Logging**: Logs request handling.
- **Metrics**: Measures execution time.

## Comparison to MediatR

MediatorFlow improves upon MediatR by:
- Automatic handler registration via source generator.
- Built-in pipeline behaviors for common cross-cutting concerns.
- Clean architecture with separation of concerns.

## License

MIT