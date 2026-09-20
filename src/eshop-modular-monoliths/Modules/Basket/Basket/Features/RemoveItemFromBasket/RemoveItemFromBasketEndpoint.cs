using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Basket.Basket.Features.RemoveItemFromBasket;

public record RemoveItemFromBasketResponse(bool IsSuccess);

public class RemoveItemFromBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/basket/{userName}/items/{productId}", async (string userName, Guid productId, ISender sender) =>
            {
                var result = await sender.Send(new RemoveItemFromBasketCommand(userName, productId));
                return Results.Ok(new RemoveItemFromBasketResponse(result.IsSuccess));
            })
            .WithName("RemoveItemFromBasket")
            .Produces<RemoveItemFromBasketResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Remove Item From Basket")
            .WithDescription("Remove Item From Basket")
            .WithTags("Basket");
    }
}