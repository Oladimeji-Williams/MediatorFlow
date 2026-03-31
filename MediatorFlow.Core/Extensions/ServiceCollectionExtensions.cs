using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediatorFlow.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediatorFlow(this IServiceCollection services, Assembly[]? assemblies = null)
    {
        assemblies ??= AppDomain.CurrentDomain.GetAssemblies();

        // Register all IRequestHandler<,>
        var requestHandlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { Interface = i, Implementation = t }));

        foreach (var handler in requestHandlerTypes)
        {
            services.AddTransient(handler.Interface, handler.Implementation);
        }

    // Register all INotificationHandler<>
    var notificationHandlerTypes = assemblies
        .SelectMany(a => a.GetTypes())
        .Where(t => !t.IsAbstract && !t.IsInterface)
        .SelectMany(t => t.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
            .Select(i => new { Interface = i, Implementation = t }));

    foreach (var handler in notificationHandlerTypes)
    {
        services.AddTransient(handler.Interface, handler.Implementation);
    }

    // Register all IPipelineBehavior<,>
    var behaviorTypes = assemblies
        .SelectMany(a => a.GetTypes())
        .Where(t => !t.IsAbstract && !t.IsInterface)
        .SelectMany(t => t.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
            .Select(i => new { Interface = i, Implementation = t }));

    foreach (var behavior in behaviorTypes)
    {
        services.AddTransient(behavior.Interface, behavior.Implementation);
    }

        // Register the Mediator
        services.AddSingleton<IMediator, Mediator>();

        return services;
    }
}