using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Basket.Basket.Features.AddItemIntoBasket;

public record AddItemIntoBasketRequest(Guid ProductId, int Quantity, string Color);

public record AddItemIntoBasketResponse(bool IsSuccess);

public class AddItemIntoBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket/{userName}/items", async (string userName, AddItemIntoBasketRequest request, ISender sender) =>
            {
                var command = new AddItemIntoBasketCommand(
                    userName, request.ProductId, request.Quantity, request.Color);

                var result = await sender.Send(command);

                return Results.Ok(new AddItemIntoBasketResponse(result.IsSuccess));
            })
            .WithName("AddItemIntoBasket")
            .Produces<AddItemIntoBasketResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Add Item Into Basket")
            .WithDescription("Add Item Into Basket")
            .WithTags("Basket");
    }
}