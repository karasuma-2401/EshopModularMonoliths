using Basket.Data;
using Catalog.Contracts.Products.Features.GetProductById;
using FluentValidation;
using MediatR;
using Shared.Contracts.CQRS;

namespace Basket.Basket.Features.AddItemIntoBasket;

public record AddItemIntoBasketCommand(string UserName, Guid ProductId, int Quantity, string Color)
    : ICommand<AddItemIntoBasketResult>;
public record AddItemIntoBasketResult(bool IsSuccess);

public class AddItemIntoBasketCommandValidator : AbstractValidator<AddItemIntoBasketCommand>
{
    public AddItemIntoBasketCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

internal class AddItemIntoBasketHandler(IBasketRepository repository, ISender sender)
    : ICommandHandler<AddItemIntoBasketCommand, AddItemIntoBasketResult>
{
    public async Task<AddItemIntoBasketResult> Handle(AddItemIntoBasketCommand command, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasket(command.UserName, false, cancellationToken);

        var result = await sender.Send(new GetProductByIdQuery(command.ProductId), cancellationToken);

        basket.AddItem(
            command.ProductId,
            command.Quantity,
            command.Color,
            result.Product.Price,
            result.Product.Name);

        await repository.SaveChangesAsync(command.UserName, cancellationToken);

        return new AddItemIntoBasketResult(true);
    }
}