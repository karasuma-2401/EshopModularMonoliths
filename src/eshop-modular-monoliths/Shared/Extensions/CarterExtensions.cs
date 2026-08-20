using System.Reflection;
using Carter;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Extensions;

// helper to register 3 services at the same time
public static class CarterExtensions
{
    public static IServiceCollection AddCarterWithAssemblies(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddCarter(configurator: config =>
        {
            foreach (var assembly in assemblies)
            {
                var modules = assembly.GetTypes()
                    .Where(t => t.IsAssignableTo(typeof(CarterModule))).ToArray();
                config.WithModules(modules);
            }
        });
        return services;
    }
    
}