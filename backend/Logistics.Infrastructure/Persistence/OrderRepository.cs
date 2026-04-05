using Logistics.Domain.Entities;
using Logistics.Domain.Enums;
using Logistics.Application.Ports;
using Logistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence;

public sealed class OrderRepository : IOrderRepository
{
    private readonly TmsDbContext _db;

    public OrderRepository(TmsDbContext db)
    {
        _db = db;
    }

    public Task<bool> DepotExistsAsync(Guid depotId, CancellationToken ct)
        => _db.Depots.AnyAsync(d => d.Id == depotId, ct);

    public async Task<(double Latitude, double Longitude)?> GetDepotCoordinatesAsync(Guid depotId, CancellationToken ct)
    {
        var d = await _db.Depots.AsNoTracking()
            .Where(x => x.Id == depotId)
            .Select(x => new { x.Latitude, x.Longitude })
            .SingleOrDefaultAsync(ct);

        return d is null ? null : (d.Latitude, d.Longitude);
    }

    public Task AddAsync(Order order, CancellationToken ct)
    {
        _db.Orders.Add(order);
        return Task.CompletedTask;
    }

    public async Task<OrderAccessData?> GetAccessDataAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
            return null;

        return await BuildAccessDataAsync(order, ct);
    }

    public async Task<IReadOnlyList<OrderAccessData>> ListByCustomerAsync(Guid customerId, CancellationToken ct)
    {
        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        var list = new List<OrderAccessData>();
        foreach (var o in orders)
            list.Add(await BuildAccessDataAsync(o, ct));

        return list;
    }

    public async Task<IReadOnlyList<OrderAccessData>> ListForDriverAsync(Guid driverId, CancellationToken ct)
    {
        var orderIds = await (
            from oa in _db.OrderAssignments.AsNoTracking()
            join r in _db.Routes.AsNoTracking() on oa.RouteId equals r.Id
            where r.DriverId == driverId
            select oa.OrderId
        ).Distinct().ToListAsync(ct);

        var orders = await _db.Orders.AsNoTracking()
            .Where(o => orderIds.Contains(o.Id))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        var list = new List<OrderAccessData>();
        foreach (var o in orders)
            list.Add(await BuildAccessDataAsync(o, ct));

        return list;
    }

    public async Task<IReadOnlyList<OrderAccessData>> ListForWarehouseBoardAsync(CancellationToken ct)
    {
        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Delivered)
            .OrderBy(o => o.RequiredDeliveryBefore ?? o.CreatedAt)
            .ToListAsync(ct);

        var list = new List<OrderAccessData>();
        foreach (var o in orders)
            list.Add(await BuildAccessDataAsync(o, ct));

        return list;
    }

    public async Task<IReadOnlyList<OrderAccessData>> ListAllAsync(CancellationToken ct)
    {
        var orders = await _db.Orders.AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        var list = new List<OrderAccessData>();
        foreach (var o in orders)
            list.Add(await BuildAccessDataAsync(o, ct));

        return list;
    }

    private async Task<OrderAccessData> BuildAccessDataAsync(Order order, CancellationToken ct)
    {
        var depotName = await _db.Depots.AsNoTracking()
            .Where(d => d.Id == order.PickupDepotId)
            .Select(d => d.Name)
            .SingleAsync(ct);

        var assignment = await _db.OrderAssignments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.OrderId == order.Id, ct);

        if (assignment is null)
        {
            return new OrderAccessData(
                order,
                depotName,
                null,
                null,
                null,
                null);
        }

        var route = await _db.Routes.AsNoTracking()
            .SingleAsync(r => r.Id == assignment.RouteId, ct);

        return new OrderAccessData(
            order,
            depotName,
            route.Id,
            route.StartTime,
            route.EndTime,
            route.DriverId);
    }
}
