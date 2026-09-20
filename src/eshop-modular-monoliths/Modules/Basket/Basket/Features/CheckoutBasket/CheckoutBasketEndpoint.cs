using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Basket.Basket.Features.CheckoutBasket;

public record CheckoutBasketResponse(bool IsSuccess);
public class CheckoutBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket/checkout", async (BasketCheckoutDto dto, ISender sender) =>
            {
                var result = await sender.Send(new CheckoutBasketCommand(dto));
                return Results.Ok(new CheckoutBasketResponse(result.IsSuccess));
            })
            .WithName("CheckoutBasket")
            .WithTags("Basket");
    }
}