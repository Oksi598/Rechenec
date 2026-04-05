using Logistics.Application.Dtos;
using Logistics.Application.Ports;
using MediatR;

namespace Logistics.Application.Queries.Handlers;

public sealed class ListWarehouseOrdersQueryHandler
    : IRequestHandler<ListWarehouseOrdersQuery, IReadOnlyList<OrderDetailDto>>
{
    private readonly IOrderRepository _orders;

    public ListWarehouseOrdersQueryHandler(IOrderRepository orders)
    {
        _orders = orders;
    }

    public async Task<IReadOnlyList<OrderDetailDto>> Handle(
        ListWarehouseOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await _orders.ListForWarehouseBoardAsync(cancellationToken);
        return rows.Select(OrderDetailMapping.ToDto).ToList();
    }
}
