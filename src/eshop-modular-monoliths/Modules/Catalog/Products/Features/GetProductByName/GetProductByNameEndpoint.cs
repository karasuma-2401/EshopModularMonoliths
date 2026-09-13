using Carter;
using Catalog.Contracts.Products.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Products.Features.GetProductByName;

public record GetProductByNameResponse(IEnumerable<ProductDto> Products);

public class GetProductByNameEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/name/{name}", async (string name, ISender sender) =>
            {
                var result = await sender.Send(new GetProductByNameQuery(name));
                return Results.Ok(new GetProductByNameResponse(result.Products));
            })
            .WithName("GetProductByName")
            .Produces<GetProductByNameResponse>(StatusCodes.Status200OK)
            .WithSummary("Get Product By Name")
            .WithDescription("Get Product By Name");
    }
}