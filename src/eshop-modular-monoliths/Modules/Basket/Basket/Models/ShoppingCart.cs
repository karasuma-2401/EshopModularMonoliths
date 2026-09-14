using Shared.DDD;

namespace Basket.Models;

public class ShoppingCart : Aggregate<Guid>
{
    public string UserName { get; private set; } = default!;
    private readonly List<ShoppingCartItem> _items = new();
    public IReadOnlyList<ShoppingCartItem> Items => _items.AsReadOnly();

    public decimal TotalPrice => Items.Sum(i => i.Price * i.Quantity);

    public static ShoppingCart Create(Guid id, string userName)
    {
        ArgumentNullException.ThrowIfNull(userName);
        return new ShoppingCart { Id = id, UserName = userName };
    }

    public void AddItem(Guid productId, int quantity, decimal price, string color, string ProductName)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            return;
        }

        _items.Add(new ShoppingCartItem(productId, quantity, color, price, ProductName));
    }
    public void RemoveItem(Guid productId)
    {
        _items.RemoveAll(i => i.ProductId == productId);
    }
    public void UpdateItemPrice(Guid ProductId, decimal Price)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == ProductId);
        if (item is not null)
        {
            item.Price = Price;
        }
    }
}

public class ShoppingCartItem
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; set; }
    public string Color { get; private set; } = default!;
    public decimal Price { get; set; }
    public string ProductName { get; private set; } = default!;

    public ShoppingCartItem(Guid productId, int quantity, string color, decimal price, string productName)
    {
        ProductId = productId;
        Quantity = quantity;
        Color = color;
        Price = price;
        ProductName = productName;
        
    }

} 