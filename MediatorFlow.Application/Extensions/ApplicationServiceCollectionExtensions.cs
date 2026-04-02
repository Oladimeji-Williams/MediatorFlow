using Microsoft.Extensions.DependencyInjection;
using MediatorFlow.Core.Abstractions;

namespace MediatorFlow.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddMediatorFlowApplication(this IServiceCollection services)
    {
        services.AddSingleton<IDispatcher, MediatorFlow.Generated.GeneratedDispatcher>();
        return services;
    }
}