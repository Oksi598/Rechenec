using Logistics.Domain.Entities;

namespace Logistics.Application.Ports;

public sealed record OrderAccessData(
    Order Order,
    string PickupDepotName,
    Guid? RouteId,
    DateTimeOffset? RouteStart,
    DateTimeOffset? RouteEnd,
    Guid? RouteDriverId);

public interface IOrderRepository
{
    Task<bool> DepotExistsAsync(Guid depotId, CancellationToken ct);

    Task<(double Latitude, double Longitude)?> GetDepotCoordinatesAsync(Guid depotId, CancellationToken ct);

    Task AddAsync(Order order, CancellationToken ct);

    Task<OrderAccessData?> GetAccessDataAsync(Guid orderId, CancellationToken ct);

    Task<IReadOnlyList<OrderAccessData>> ListByCustomerAsync(Guid customerId, CancellationToken ct);

    Task<IReadOnlyList<OrderAccessData>> ListForDriverAsync(Guid driverId, CancellationToken ct);

    Task<IReadOnlyList<OrderAccessData>> ListForWarehouseBoardAsync(CancellationToken ct);

    Task<IReadOnlyList<OrderAccessData>> ListAllAsync(CancellationToken ct);
}
