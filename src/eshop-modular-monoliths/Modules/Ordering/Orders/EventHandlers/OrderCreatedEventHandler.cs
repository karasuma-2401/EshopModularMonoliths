using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Orders.Events;

namespace Ordering.Orders.EventHandlers;

public class OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    : INotificationHandler<OrderCreatedEvent>
{
    public Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event handled: {DomainEvent} for OrderId {OrderId} (Customer {CustomerId})",
            notification.GetType().Name, notification.Order.Id, notification.Order.CustomerId);
        return Task.CompletedTask;
    }
}
