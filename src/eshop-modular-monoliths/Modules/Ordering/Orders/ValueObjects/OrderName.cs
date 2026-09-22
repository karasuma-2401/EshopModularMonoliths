namespace Ordering.Orders.ValueObjects;

public record OrderName
{
    public string Value { get;  }
    private OrderName (string value) => Value = value;

    public static OrderName Of(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        return new OrderName(value);
    }
    public static OrderName Create() => new($"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}");
    
    public override string ToString() => Value;
}