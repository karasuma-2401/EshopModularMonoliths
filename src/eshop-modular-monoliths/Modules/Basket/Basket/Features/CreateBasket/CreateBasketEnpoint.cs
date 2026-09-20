using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Basket.Basket.Features.CreateBasket;

public record CreateBasketRequest(string UserName);
public record CreateBasketResponse(Guid Id);

public class CreateBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket", async (CreateBasketRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateBasketCommand(request.UserName));
            return Results.Created($"/basket/{result.Id}", new CreateBasketResponse(result.Id));
        }).WithName("CreateBasket").WithTags("Basket");
    }
}