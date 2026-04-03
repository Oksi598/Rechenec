using Logistics.Application.Ports;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.LoadOptimization;

public sealed class LoadOptimizationDataProvider : ILoadOptimizationDataProvider
{
    private readonly TmsDbContext _db;

    public LoadOptimizationDataProvider(TmsDbContext db)
    {
        _db = db;
    }

    public async Task<LoadOptimizationData> GetDataAsync(Guid routeId, CancellationToken ct)
    {
        var route = await _db.Routes
            .SingleAsync(r => r.Id == routeId, ct);

        var vehicle = await _db.Vehicles
            .SingleAsync(v => v.Id == route.VehicleId, ct);

        var orderIds = await _db.OrderAssignments
            .Where(oa => oa.RouteId == routeId)
            .Select(oa => oa.OrderId)
            .ToListAsync(ct);

        if (orderIds.Count == 0)
            return new LoadOptimizationData(vehicle, new GeoCoordinate(0, 0), new List<Order>());

        var orders = await _db.Orders
            .Where(o => orderIds.Contains(o.Id))
            .ToListAsync(ct);

        var depotId = orders[0].PickupDepotId;
        var depot = await _db.Depots
            .SingleAsync(d => d.Id == depotId, ct);

        var depotCoord = new GeoCoordinate(depot.Latitude, depot.Longitude);
        return new LoadOptimizationData(vehicle, depotCoord, orders);
    }
}
