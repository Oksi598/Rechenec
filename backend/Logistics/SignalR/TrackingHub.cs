using Logistics.Domain;
using Logistics.Domain.Entities;
using Logistics.Api.Observability;
using Logistics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Logistics.Api.SignalR;

[Authorize]
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
            await EnsureCanAccessRouteAsync(routeId, default);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{GroupPrefix}{routeId}");
        }
        else
        {
            throw new HubException("Missing or invalid routeId query parameter.");
        }

        await base.OnConnectedAsync();
    }

    public async Task UpdateVehicleLocation(UpdateVehicleLocationDto dto, CancellationToken ct = default)
    {
        await EnsureCanAccessRouteAsync(dto.RouteId, ct);
        var now = DateTimeOffset.UtcNow;

        var location = new VehicleLocation
        {
            Id = Guid.NewGuid(),
            VehicleId = dto.VehicleId,
            Latitude = dto.Lat,
            Longitude = dto.Lng,
            Speed = dto.Speed ?? 0,
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
        TmsMetrics.TrackTrackingUpdate();
    }

    private async Task EnsureCanAccessRouteAsync(Guid routeId, CancellationToken ct)
    {
        if (Context.User is null)
            throw new HubException("Unauthorized.");

        if (Context.User.IsInRole(AppRoles.Dispatcher))
            return;

        var userIdValue = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new HubException("Unauthorized.");

        var hasAccess = await _db.Routes.AnyAsync(r => r.Id == routeId && r.DriverId == userId, ct);
        if (!hasAccess)
            throw new HubException("Forbidden for this route.");
    }
}

public sealed record UpdateVehicleLocationDto(
    Guid RouteId,
    Guid VehicleId,
    double Lat,
    double Lng,
    double? Speed);
