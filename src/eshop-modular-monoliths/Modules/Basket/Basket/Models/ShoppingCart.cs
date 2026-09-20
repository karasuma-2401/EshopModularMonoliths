using System.Text.Json.Serialization;
using Shared.DDD;

namespace Basket.Models;

public class ShoppingCart : Aggregate<Guid>
{
    public string UserName { get; private set; } = default!;

    private readonly List<ShoppingCartItem> _items = new();
    public IReadOnlyList<ShoppingCartItem> Items => _items.AsReadOnly();

    public decimal TotalPrice => Items.Sum(i => i.Price * i.Quantity);
    
    // Constructor mặc định cần thiết cho EF Core
    private ShoppingCart() { }
    
    [JsonConstructor]
    public ShoppingCart(Guid id, string userName, List<ShoppingCartItem>? items = null)
    {
        Id = id;
        UserName = userName;
        if (items != null)
        {
            _items = items;
        }
    }
    
    public static ShoppingCart Create(Guid id, string userName)
    {
        ArgumentException.ThrowIfNullOrEmpty(userName);
        return new ShoppingCart(id, userName);
    }

    public void AddItem(Guid productId, int quantity, string color, decimal price, string productName)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            return;
        }

        _items.Add(new ShoppingCartItem(Id, productId, quantity, color, price, productName));
    }

    public void RemoveItem(Guid productId)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            _items.Remove(existingItem);
        }
    }

    public void UpdateItemPrice(Guid productId, decimal price)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            item.Price = price;
        }
    }
}

public class ShoppingCartItem
{
    public Guid Id { get; set; }
    public Guid ShoppingCartId { get; set; } = default!;
    public Guid ProductId { get; set; } = default!;
    public int Quantity { get; set; } = default!;
    public string Color { get; set; } = default!;
    public decimal Price { get; set; } = default!;
    public string ProductName { get; set; } = default!;

    public ShoppingCartItem() { }

    public ShoppingCartItem(Guid shoppingCartId, Guid productId, int quantity, string color, decimal price, string productName)
    {
        ShoppingCartId = shoppingCartId;
        ProductId = productId;
        Quantity = quantity;
        Color = color;
        Price = price;
        ProductName = productName;
    }

    [JsonConstructor]
    public ShoppingCartItem(Guid id, Guid shoppingCartId, Guid productId, int quantity, string color, decimal price, string productName)
    {
        Id = id;
        ShoppingCartId = shoppingCartId;
        ProductId = productId;
        Quantity = quantity;
        Color = color;
        Price = price;
        ProductName = productName;
    }
}