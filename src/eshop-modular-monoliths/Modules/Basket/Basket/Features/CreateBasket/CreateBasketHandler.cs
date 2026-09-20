using Basket.Data;
using Basket.Models;
using FluentValidation;
using Shared.Contracts.CQRS;
using Shared.Exceptions;

namespace Basket.Basket.Features.CreateBasket;

public record CreateBasketCommand(string UserName) : ICommand<CreateBasketResult>;
public record CreateBasketResult(Guid Id);

public class CreateBasketCommandValidator : AbstractValidator<CreateBasketCommand>
{
    public CreateBasketCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("Username is required.");
    }
}

internal class CreateBasketHandler(IBasketRepository repository)
    : ICommandHandler<CreateBasketCommand, CreateBasketResult>
{
    public async Task<CreateBasketResult> Handle(CreateBasketCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await repository.GetBasket(command.UserName, true, cancellationToken);
            return new CreateBasketResult(existing.Id);
        }
        catch (BasketNotFoundException)
        {
        }

        var basket = ShoppingCart.Create(Guid.NewGuid(), command.UserName);
        await repository.CreateBasket(basket, cancellationToken);

        return new CreateBasketResult(basket.Id);
    }
}