using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Behaviors;
using MediatorFlow.Core.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace MediatorFlow.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediatorFlow(this IServiceCollection services, params Assembly[] assemblies)
    {
        assemblies ??= new[] { Assembly.GetCallingAssembly() };

        var types = assemblies.SelectMany(a =>
        {
            try { return a.GetTypes(); }
            catch { return Array.Empty<Type>(); }
        });

        // IRequestHandler
        var requestHandlers = types
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { Interface = i, Implementation = t }))
            .GroupBy(x => new { x.Interface, x.Implementation })
            .Select(g => g.First());

        foreach (var handler in requestHandlers)
            services.AddTransient(handler.Interface, handler.Implementation);

        // INotificationHandler
        var notificationHandlers = types
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .Select(i => new { Interface = i, Implementation = t }))
            .GroupBy(x => new { x.Interface, x.Implementation })
            .Select(g => g.First());

        foreach (var handler in notificationHandlers)
            services.AddTransient(handler.Interface, handler.Implementation);

        // Pipeline (ordered)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));



        // Mediator
        services.AddSingleton<IMediator, Mediator>();
        return services;

    }
}

