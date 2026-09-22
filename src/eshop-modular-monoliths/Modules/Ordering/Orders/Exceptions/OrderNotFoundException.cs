namespace Ordering.Orders.Exceptions;

public class OrderNotFoundException(Guid orderId)
    : Exception($"Order with id '{orderId}' was not found.");