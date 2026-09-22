using Basket.Basket.Features.UpdateItemPriceInBasket;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events;

namespace Basket.Basket.EventHandlers;

public class ProductPriceChangedIntegrationEventHandler(
    ISender sender,
    ILogger<ProductPriceChangedIntegrationEventHandler> logger)
    : IConsumer<ProductPriceChangedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductPriceChangedIntegrationEvent> context)
    {
        logger.LogInformation("Integration Event handled: {IntegrationEvent} for ProductId {ProductId}", 
            context.Message.GetType().Name, context.Message.ProductId);

        var command = new UpdateItemPriceInBasketCommand(context.Message.ProductId, context.Message.Price);
        var result = await sender.Send(command);

        if (!result.IsSuccess)
        {
            logger.LogWarning("No shopping cart items found to update price for product id: {ProductId}", 
                context.Message.ProductId);
            return;
        }

        logger.LogInformation("Price for product id: {ProductId} updated successfully in basket", 
            context.Message.ProductId);
    }
}
