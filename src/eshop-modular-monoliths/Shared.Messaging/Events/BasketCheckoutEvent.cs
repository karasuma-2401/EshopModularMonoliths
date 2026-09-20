namespace Shared.Messaging.Events;

public record BasketCheckoutEvent : IntergrationEvent
{
    public string UserName { get; set; } = default!;
    public decimal TotalPrice { get; set; }
    public List<BasketCheckoutItem> Items { get; set; } = new();
    
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string AddressLine { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string State { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public string CardName { get; set; } = default!;
    public string CardNumber { get; set; } = default!;
    public string Expiration { get; set; } = default!;
    public string Cvv { get; set; } = default!;
    public int PaymentMethod { get; set; }
}
public record BasketCheckoutItem(Guid ProductId, int Quantity, decimal Price, string ProductName);