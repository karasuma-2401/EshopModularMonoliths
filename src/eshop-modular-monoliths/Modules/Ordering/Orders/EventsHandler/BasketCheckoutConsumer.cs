using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Orders.Dtos;
using Ordering.Orders.Features.CreateOrder;
using Shared.Messaging.Events;


namespace Ordering.Orders.EventsHandler;

public class BasketCheckoutConsumer(ISender sender, ILogger<BasketCheckoutConsumer> logger) 
    : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Consuming BasketCheckoutEvent for user {UserName}", message.UserName);
        
        var addressDto = new AddressDto(
            message.FirstName, message.LastName, message.EmailAddress,
            message.AddressLine, message.Country, message.State, message.ZipCode);
        var paymentDto = new PaymentDto(
            message.CardName, message.CardNumber, message.Expiration, message.Cvv, message.PaymentMethod);
        var orderId = Guid.NewGuid();
        var orderItems = message.Items.Select(item => new OrderItemDto(
            orderId,
            item.ProductId,
            item.Quantity,
            item.Price,
            item.ProductName)).ToList();
        var orderDto = new OrderDto(
            Id: orderId,
            CustomerId: Guid.NewGuid(),
            OrderName: message.UserName,
            ShippingAddress: addressDto,
            BillingAddress: addressDto,
            Payment: paymentDto,
            Items: orderItems);
        
        await sender.Send(new CreateOrderCommand(orderDto), context.CancellationToken);
        logger.LogInformation("Order created successfully for user {UserName}", message.UserName);
    }
}