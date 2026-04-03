using Logistics.Application.Ports;
using Logistics.Domain.Entities;
using Logistics.Domain.Enums;
using Logistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

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
        {
            var emptyPoint = new Point(0, 0) { SRID = 4326 };
            return new LoadOptimizationData(vehicle, emptyPoint, new List<Order>());
        }

        // Include all orders assigned to route for deterministic occupancy accumulation.
        // Service will move statuses according to LTL rule.
        var orders = await _db.Orders
            .Where(o => orderIds.Contains(o.Id))
            .ToListAsync(ct);

        // Use first order's pickup depot as the distance anchor.
        var depotId = orders[0].PickupDepotId;
        var depot = await _db.Depots
            .SingleAsync(d => d.Id == depotId, ct);

        // Ensure SRID is set for deterministic Haversine computations.
        var depotLocation = depot.Location;
        if (depotLocation.SRID == 0)
            depotLocation = new Point(depotLocation.X, depotLocation.Y) { SRID = 4326 };

        return new LoadOptimizationData(vehicle, depotLocation, orders);
    }
}

