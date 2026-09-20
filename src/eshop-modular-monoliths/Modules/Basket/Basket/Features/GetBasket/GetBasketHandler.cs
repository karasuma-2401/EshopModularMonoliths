using Basket.Data;
using Basket.Models;
using Shared.Contracts.CQRS;

namespace Basket.Basket.Features.GetBasket;

public record GetBasketQuery(string UserName) : IQuery<GetBasketResult>;
public record GetBasketResult(ShoppingCart Cart);

internal class GetBasketHandler(IBasketRepository repository) 
    : IQueryHandler<GetBasketQuery, GetBasketResult>
{
    public async Task<GetBasketResult> Handle(GetBasketQuery command, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasket(command.UserName, true, cancellationToken);
        return new GetBasketResult(basket);
    }
}