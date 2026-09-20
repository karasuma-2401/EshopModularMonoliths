using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Data.Outbox;

public class OutboxProcessorJob<TContext>(
    IServiceProvider serviceProvider,
    ILogger<OutboxProcessorJob<TContext>> logger) : BackgroundService
    where TContext : DbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                var messages = await dbContext.Set<OutboxMessage>()
                    .Where(x => x.ProcessedOn == null)
                    .OrderBy(x => x.OccurredOn)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    var type = Type.GetType(message.Type);
                    if (type == null)
                    {
                        logger.LogWarning("Could not resolve type: {Type}", message.Type);
                        continue;
                    }

                    var @event = JsonSerializer.Deserialize(message.Content, type);
                    if (@event is null)
                    {
                        logger.LogWarning("Could not deserialize message: {Content}", message.Content);
                        continue;
                    }

                    await publishEndpoint.Publish(@event, type, stoppingToken);

                    message.ProcessedOn = DateTime.UtcNow;
                    logger.LogInformation("Successfully processed outbox message with ID: {Id}", message.Id);
                }

                if (messages.Count > 0)
                    await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}