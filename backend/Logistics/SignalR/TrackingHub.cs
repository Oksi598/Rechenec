using Logistics.Domain.Entities;
using Logistics.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using NetTopologySuite.Geometries;

namespace Logistics.Api.SignalR;

public sealed class TrackingHub : Hub
{
    private const string GroupPrefix = "route:";

    private readonly TmsDbContext _db;

    public TrackingHub(TmsDbContext db)
    {
        _db = db;
    }

    public override async Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext();
        var routeIdValue = http?.Request.Query["routeId"].ToString();

        if (Guid.TryParse(routeIdValue, out var routeId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{GroupPrefix}{routeId}");
        }

        await base.OnConnectedAsync();
    }

    public async Task UpdateVehicleLocation(UpdateVehicleLocationDto dto, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Persisting coordinates can be expensive; for a blueprint we write a row per update.
        // (In production: throttle/batch per vehicle.)
        var location = new VehicleLocation
        {
            Id = Guid.NewGuid(),
            VehicleId = dto.VehicleId,
            Location = new Point(dto.Lng, dto.Lat) { SRID = 4326 },
            Speed = (decimal)(dto.Speed ?? 0),
            RecordedAt = now
        };

        _db.VehicleLocations.Add(location);
        await _db.SaveChangesAsync(ct);

        var payload = new
        {
            dto.VehicleId,
            dto.Lat,
            dto.Lng,
            dto.Speed,
            RecordedAt = now
        };

        await Clients.Group($"{GroupPrefix}{dto.RouteId}")
            .SendAsync("VehicleLocationChanged", payload, ct);
    }
}

public sealed record UpdateVehicleLocationDto(
    Guid RouteId,
    Guid VehicleId,
    double Lat,
    double Lng,
    double? Speed);

