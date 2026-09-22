using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Messaging.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitWithAssemblies(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            config.SetInMemorySagaRepositoryProvider();

            config.AddConsumers(assemblies);
            config.AddSagaStateMachines(assemblies);
            config.AddSagas(assemblies);
            config.AddActivities(assemblies);

            config.UsingRabbitMq((context, configurator) =>
            {
                var host = configuration["MessageBroker:Host"];
                if (!string.IsNullOrEmpty(host))
                {
                    var hostUri = new Uri(host);
                    configurator.Host(hostUri, h =>
                    {
                        var userInfo = hostUri.UserInfo;
                        var username = configuration["MessageBroker:UserName"] 
                            ?? configuration["MessageBroker:Username"]
                            ?? (!string.IsNullOrEmpty(userInfo) ? userInfo.Split(':')[0] : null);
                        var password = configuration["MessageBroker:Password"]
                            ?? (!string.IsNullOrEmpty(userInfo) && userInfo.Contains(':') ? userInfo.Split(':')[1] : null);

                        if (!string.IsNullOrEmpty(username)) h.Username(username);
                        if (!string.IsNullOrEmpty(password)) h.Password(password);
                    });
                }
                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
