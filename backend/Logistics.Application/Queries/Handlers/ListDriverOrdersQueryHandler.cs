using Logistics.Application.Dtos;
using Logistics.Application.Ports;
using MediatR;

namespace Logistics.Application.Queries.Handlers;

public sealed class ListDriverOrdersQueryHandler
    : IRequestHandler<ListDriverOrdersQuery, IReadOnlyList<OrderDetailDto>>
{
    private readonly IOrderRepository _orders;
    private readonly ICurrentUser _currentUser;

    public ListDriverOrdersQueryHandler(IOrderRepository orders, ICurrentUser currentUser)
    {
        _orders = orders;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<OrderDetailDto>> Handle(
        ListDriverOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var rows = await _orders.ListForDriverAsync(userId, cancellationToken);
        return rows.Select(OrderDetailMapping.ToDto).ToList();
    }
}
