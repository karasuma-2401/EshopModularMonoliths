using System.Text.Json;
using Basket.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Contracts.CQRS;
using Shared.Data.Outbox;
using Shared.Exceptions;
using Shared.Messaging.Events;

namespace Basket.Basket.Features.CheckoutBasket;

public record CheckoutBasketCommand(BasketCheckoutDto BasketCheckoutDto) : ICommand<CheckoutBasketResult>;
public record CheckoutBasketResult(bool IsSuccess);

public record BasketCheckoutDto
{
    public string UserName { get; set; } = default!;
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

public class CheckoutBasketCommandValidator : AbstractValidator<CheckoutBasketCommand>
{
    public CheckoutBasketCommandValidator()
    {
        RuleFor(x => x.BasketCheckoutDto.UserName).NotEmpty();
        RuleFor(x => x.BasketCheckoutDto.CardNumber).NotEmpty();
    }
}

internal class CheckoutBasketHandler(BasketDbContext dbContext, IDistributedCache cache)
    : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        var dto = command.BasketCheckoutDto;
        var basket = await dbContext.ShoppingCarts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.UserName == dto.UserName, cancellationToken);

        if (basket is null)
            throw new BasketNotFoundException(dto.UserName);

        var integrationEvent = new BasketCheckoutEvent
        {
            UserName = basket.UserName,
            TotalPrice = basket.TotalPrice,
            Items = basket.Items.Select(x =>
                new BasketCheckoutItem(x.ProductId, x.Quantity, x.Price, x.ProductName)).ToList(),

            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmailAddress = dto.EmailAddress,
            AddressLine = dto.AddressLine,
            Country = dto.Country,
            State = dto.State,
            ZipCode = dto.ZipCode,
            CardName = dto.CardName,
            CardNumber = dto.CardNumber,
            Expiration = dto.Expiration,
            Cvv = dto.Cvv,
            PaymentMethod = dto.PaymentMethod
        };
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(BasketCheckoutEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(integrationEvent),
            OccurredOn = DateTime.UtcNow
        };

        dbContext.OutboxMessages.Add(outboxMessage);
        dbContext.ShoppingCarts.Remove(basket);

        await dbContext.SaveChangesAsync(cancellationToken);   // ← 1 transaction duy nhất, atomic
        await cache.RemoveAsync(dto.UserName, cancellationToken);

        return new CheckoutBasketResult(true);
    }
}