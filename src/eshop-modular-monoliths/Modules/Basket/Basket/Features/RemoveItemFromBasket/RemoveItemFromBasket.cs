using Basket.Data;
using Shared.Contracts.CQRS;

namespace Basket.Basket.Features.RemoveItemFromBasket;

public record RemoveItemFromBasketCommand(string UserName, Guid ProductId) : ICommand<RemoveItemFromBasketResult>;
public record RemoveItemFromBasketResult(bool IsSuccess);

internal class RemoveItemFromBasketHandler(IBasketRepository repository)
    : ICommandHandler<RemoveItemFromBasketCommand, RemoveItemFromBasketResult>
{
    public async Task<RemoveItemFromBasketResult> Handle(RemoveItemFromBasketCommand command, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasket(command.UserName, false, cancellationToken);
        basket.RemoveItem(command.ProductId);
        await repository.SaveChangesAsync(command.UserName, cancellationToken);
        return new RemoveItemFromBasketResult(true);
    }
}