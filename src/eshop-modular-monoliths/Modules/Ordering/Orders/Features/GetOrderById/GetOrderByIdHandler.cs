using Mapster;
using Microsoft.EntityFrameworkCore;
using Ordering.Data;
using Ordering.Orders.Dtos;
using Ordering.Orders.Exceptions;
using Ordering.Orders.Features.GetOrders;
using Shared.Contracts.CQRS;

namespace Ordering.Orders.Features.GetOrderById;


public record GetOrderByIdQuery(Guid Id) 
    : IQuery<GetOrderByIdResult>;
public record GetOrderByIdResult(OrderDto Order);


internal class GetOrderByIdHandler(OrderingDbContext dbContext)
    : IQueryHandler<GetOrderByIdQuery, GetOrderByIdResult>
{
    public async Task<GetOrderByIdResult> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .SingleOrDefaultAsync(p => p.Id == query.Id, cancellationToken);
        if (order == null)
        {
            throw new OrderNotFoundException(query.Id);
        }
        var orderDto = order.Adapt<OrderDto>();
        return new GetOrderByIdResult(orderDto);
    }
}
