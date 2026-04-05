using Logistics.Application.Dtos;
using Logistics.Application.Ports;
using MediatR;

namespace Logistics.Application.Queries.Handlers;

public sealed class ListDispatcherOrdersQueryHandler
    : IRequestHandler<ListDispatcherOrdersQuery, IReadOnlyList<OrderDetailDto>>
{
    private readonly IOrderRepository _orders;

    public ListDispatcherOrdersQueryHandler(IOrderRepository orders)
    {
        _orders = orders;
    }

    public async Task<IReadOnlyList<OrderDetailDto>> Handle(
        ListDispatcherOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await _orders.ListAllAsync(cancellationToken);
        return rows.Select(OrderDetailMapping.ToDto).ToList();
    }
}
