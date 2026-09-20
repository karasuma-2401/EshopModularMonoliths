namespace Shared.Exceptions;

public class BasketNotFoundException(string userName)
    : Exception($"Basket with username {userName} was not found.");