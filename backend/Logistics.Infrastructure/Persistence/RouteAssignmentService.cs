using Logistics.Application.Ports;
using Logistics.Domain.Entities;
using Logistics.Domain.Enums;
using Logistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence;

public sealed class RouteAssignmentService : IRouteAssignmentService
{
    private readonly TmsDbContext _db;

    public RouteAssignmentService(TmsDbContext db)
    {
        _db = db;
    }

    public async Task<RouteAssignmentSnapshot> UpdateAssignmentAsync(
        Guid routeId,
        Guid vehicleId,
        Guid driverId,
        byte[] expectedRowVersion,
        CancellationToken ct)
    {
        var route = await _db.Routes.SingleOrDefaultAsync(r => r.Id == routeId, ct)
            ?? throw new KeyNotFoundException($"Route '{routeId}' was not found.");

        var isVehicleInUse = await _db.Routes
            .AnyAsync(r =>
                r.Id != routeId &&
                r.VehicleId == vehicleId &&
                (r.Status == RouteStatus.Planned || r.Status == RouteStatus.Active),
                ct);

        if (isVehicleInUse)
            throw new VehicleAlreadyAssignedException(vehicleId);

        _db.Entry(route).Property(x => x.RowVersion).OriginalValue = expectedRowVersion;

        route.VehicleId = vehicleId;
        route.DriverId = driverId;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new RouteConcurrencyException("Route assignment has been changed by another process.", ex);
        }

        return new RouteAssignmentSnapshot(route.Id, route.VehicleId, route.DriverId, route.RowVersion);
    }
}
