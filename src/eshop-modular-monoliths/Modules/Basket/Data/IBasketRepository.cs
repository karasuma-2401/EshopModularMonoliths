using Basket.Models;

namespace Basket.Data;

public interface IBasketRepository
{
    Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default);
    Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default);
    Task DeleteBasket(string userName, CancellationToken cancellationToken = default);
}