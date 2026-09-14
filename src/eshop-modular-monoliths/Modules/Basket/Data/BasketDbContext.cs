using System.Reflection;
using Basket.Models;
using Microsoft.EntityFrameworkCore;

namespace Basket.Data;

public class BasketDbContext(DbContextOptions<BasketDbContext> options) : DbContext(options)
{
    public   DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("basket");
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}