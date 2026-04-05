using Logistics.Application.Dtos;
using Logistics.Application.Ports;
using Logistics.Domain;
using MediatR;

namespace Logistics.Application.Queries.Handlers;

public sealed class GetOrderForCurrentUserQueryHandler
    : IRequestHandler<GetOrderForCurrentUserQuery, OrderDetailDto?>
{
    private readonly IOrderRepository _orders;
    private readonly ICurrentUser _currentUser;

    public GetOrderForCurrentUserQueryHandler(IOrderRepository orders, ICurrentUser currentUser)
    {
        _orders = orders;
        _currentUser = currentUser;
    }

    public async Task<OrderDetailDto?> Handle(GetOrderForCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var data = await _orders.GetAccessDataAsync(request.OrderId, cancellationToken);
        if (data is null)
            return null;

        if (!CanView(userId, data))
            return null;

        return OrderDetailMapping.ToDto(data);
    }

    private bool CanView(Guid userId, OrderAccessData data)
    {
        if (_currentUser.IsInRole(AppRoles.Dispatcher))
            return true;

        if (_currentUser.IsInRole(AppRoles.Warehouse))
            return true;

        if (_currentUser.IsInRole(AppRoles.Customer) && data.Order.CustomerId == userId)
            return true;

        if (_currentUser.IsInRole(AppRoles.Driver) &&
            data.RouteDriverId.HasValue &&
            data.RouteDriverId.Value == userId)
            return true;

        return false;
    }
}
