using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Behaviors;

namespace Shared.Extensions;

// helper to register 3 services at the same time
public static class MediatRExtentions
{
    public static IServiceCollection AddMediatRWithAssemblies(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        services.AddValidatorsFromAssemblies(assemblies);
        return services;
    }
}