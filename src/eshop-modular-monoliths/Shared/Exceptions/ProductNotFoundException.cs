namespace Shared.Exceptions;

public class ProductNotFoundException(Guid id)
    : Exception($"Product with id {id} was not found.");