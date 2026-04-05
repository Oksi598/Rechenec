using Logistics.Application.Dtos;
using Logistics.Application.Ports;
using MediatR;

namespace Logistics.Application.Queries.Handlers;

public sealed class ListMyCustomerOrdersQueryHandler
    : IRequestHandler<ListMyCustomerOrdersQuery, IReadOnlyList<OrderDetailDto>>
{
    private readonly IOrderRepository _orders;
    private readonly ICurrentUser _currentUser;

    public ListMyCustomerOrdersQueryHandler(IOrderRepository orders, ICurrentUser currentUser)
    {
        _orders = orders;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<OrderDetailDto>> Handle(
        ListMyCustomerOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var rows = await _orders.ListByCustomerAsync(userId, cancellationToken);
        return rows.Select(OrderDetailMapping.ToDto).ToList();
    }
}
