using Basket.Models;
using Microsoft.EntityFrameworkCore;

namespace Basket.Data;

public class BasketRepository(BasketDbContext dbContext) : IBasketRepository
{
    public async Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default)
    {
        var basket = await dbContext.ShoppingCarts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);

        if (basket is null)
            return null;
            // throw new BasketNotFoundException(userName);

        return basket;
    }
    public async Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        dbContext.ShoppingCarts.Update(basket);
        await dbContext.SaveChangesAsync(cancellationToken);
        return basket;
    }
    public async Task DeleteBasket(string userName, CancellationToken cancellationToken = default)
    {
        var basket = await GetBasket(userName, cancellationToken);
        dbContext.ShoppingCarts.Remove(basket);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}