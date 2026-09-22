using Ordering.Orders.Enums;
using Ordering.Orders.Events;
using Ordering.Orders.ValueObjects;
using Shared.DDD;

namespace Ordering.Orders.Models;

public class Order : Aggregate<Guid>
{
    public OrderName OrderName { get; private set; } = default!;
    public Guid CustomerId { get; private set; }
    public Address ShippingAddress { get; private set; } = default!;
    public Address BillingAddress { get; private set; } = default!;
    public Payment Payment { get; private set; } = default!;

    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalPrice => _items.Sum(i => i.Price * i.Quantity);
    
    private Order() { }

    public static Order Create(Guid id, Guid customerId, OrderName orderName, Address shippingAddress,
        Address billingAddress, Payment payment)
    {
        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            OrderName = orderName,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Payment = payment,
            Status = OrderStatus.Pending
        };

        order.AddDomainEvent(new OrderCreatedEvent(order));
        return order;
    }

    public void Add(Guid productId, int quantity, decimal price, string productName)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            return;
        }
        _items.Add(new OrderItem(Id, productId, quantity, price, productName));
    }
}

public class OrderItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ProductName { get; private set; } = default!;
    
    private OrderItem() { }

    public OrderItem(Guid orderId, Guid productId, int quantity, decimal price, string productName)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
        ProductName = productName;
    }
}